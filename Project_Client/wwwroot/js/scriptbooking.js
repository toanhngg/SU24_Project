<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js" crossorigin="anonymous"></script>

async function fetchBookings() {
    try {
        const response = await fetch('https://localhost:7130/api/Booking');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return await response.json();
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
        return []; // Trả về mảng rỗng nếu có lỗi
    }
}

async function populateBookingTable() {
    const bookings = await fetchBookings();
    const tableBody = document.getElementById('bookingTableBody');
    tableBody.innerHTML = ''; // Xóa nội dung hiện tại của bảng
    bookings.forEach(booking => {
        const row = document.createElement('tr');
        row.innerHTML = `
                                        <td>${booking.id}</td>
                                        <td>${booking.customerId}</td>
                                        <td>${new Date(booking.date).toLocaleDateString()}</td>
                                        <td>${booking.time}</td>
                                        <td>${booking.numberOfPeople}</td>
                                        <td>${booking.note}</td>
                                        <td>${new Date(booking.dateBooking).toLocaleDateString()}</td>
                                        <td>${new Date(booking.dateStart).toLocaleDateString()}</td>
                                        <td>${new Date(booking.dateCheckOut).toLocaleDateString()}</td>
                                        <td>${booking.bookingTable}</td>
                                        <td>${booking.isCheck ? 'Checked' : 'Not Checked'}</td>
                                        <td><button onclick="openModal(${booking.id})">Detail</button></td>
                                            `;
        tableBody.appendChild(row);
    });
}

document.getElementById('dashboardLink').addEventListener('click', async function () {
    const bookingTable = document.getElementById('bookingTable');
    if (bookingTable.style.display === 'none' || bookingTable.style.display === '') {
        bookingTable.style.display = 'block';
        await populateBookingTable();
    } else {
        bookingTable.style.display = 'none';
    }
});

let tablesData = [];

document.addEventListener('DOMContentLoaded', () => {
    populateTables();
    populateTable(); // Assuming this is another function that populates the main table
});

function populateTables() {
    const apiUrl = 'https://localhost:7135/api/Table'; // Thay đổi URL thành URL API của bạn

    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
            tablesData = data; // Save the table data to a global variable
            const tableSelect = document.getElementById('bookingTable2');
            data.forEach(table => {
                const option = document.createElement('option');
                option.value = table.id;
                option.textContent = table.name; // Giả sử table có thuộc tính name
                tableSelect.appendChild(option);
            });
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

function openModal(id) {
    const apiUrl = `https://localhost:7130/api/Booking/getDetailById/${id}`;

    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            const booking = data[0];

            function formatDateTime(value) {
                if (value) {
                    const date = new Date(value);
                    return isNaN(date.getTime()) ? '' : date.toISOString().slice(0, 16);
                }
                return '';
            }

            document.getElementById('date').value = booking.date ? new Date(booking.date).toISOString().split('T')[0] : '';
            document.getElementById('time').value = booking.time ? booking.time.slice(0, 5) : '';
            document.getElementById('numberOfPeople').value = booking.numberOfPeople || '';
            document.getElementById('note').value = booking.note || '';
            document.getElementById('dateStart').value = formatDateTime(booking.dateStart);
            document.getElementById('dateCheckOut').value = formatDateTime(booking.dateCheckOut);

            // Tìm tên bảng dựa trên ID
            const table = tablesData.find(t => t.id === booking.bookingTable);
            if (table) {
                document.getElementById('bookingTable2').value = table.id;
            } else {
                document.getElementById('bookingTable2').value = '';
            }

            document.getElementById('isCheck').checked = booking.isCheck === true;
            document.getElementById('bookingId').value = booking.id;

            document.getElementById('detailModal').style.display = 'block';
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

function closeModal() {
    document.getElementById('detailModal').style.display = 'none';
}

function updateBooking() {
    const id = parseInt(document.getElementById('bookingId').value);
    const dateStart = document.getElementById('dateStart').value;
    const dateCheckOut = document.getElementById('dateCheckOut').value;
    const bookingTable = document.getElementById('bookingTable2').value;
    const isCheck = document.getElementById('isCheck').checked;

    // Find and update the booking
    const booking = bookings.find(b => b.id === id);

    if (booking) {
        booking.dateStart = dateStart;
        booking.dateCheckOut = dateCheckOut;
        booking.bookingTable = bookingTable;
        booking.isCheck = isCheck;

        // Optionally, you can send the updated data to the server here

        // Close the modal and refresh the table
        closeModal();
        populateTable();
    }
}