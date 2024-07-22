const apiUrl = 'https://localhost:7135/api/Products';
let productList = [];
let authToken = '';

async function fetchProducts() {
    try {
        const response = await fetch(apiUrl);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();
        productList = data;
        updateSelectors();
    } catch (error) {
        console.error('There has been a problem with your fetch operation:', error);
    }
}

function updateSelectors() {
    const selectors = document.querySelectorAll('.item-select');
    selectors.forEach(selector => {
        if (selector) {
            const selectedValue = selector.value;
            selector.innerHTML = '<option value="" data-price="0">Select Item</option>';
            productList.forEach(product => {
                const option = document.createElement('option');
                option.value = product.id;
                option.textContent = `${product.name} - $${product.price}`;
                option.setAttribute('data-price', product.price);
                if (product.id == selectedValue) {
                    option.selected = true;
                }
                selector.appendChild(option);
            });
        }
    });
}

function updatePrice(element) {
    const row = element.closest('.order-item');
    const price = row.querySelector('.item-select option:checked').getAttribute('data-price');
    const quantity = row.querySelector('.quantity').value;
    row.querySelector('.price').value = '$' + (price * quantity).toFixed(2);
    updateTotal();
}

function addItem() {
    const orderItems = document.getElementById('order-items');
    const newItem = document.createElement('div');
    newItem.className = 'row gy-4 order-item';
    newItem.innerHTML = `
        <div class="col-lg-5 col-md-6">
            <select class="form-control item-select" name="item[]" onchange="updatePrice(this)">
                <option value="" data-price="0">Select Item</option>
            </select>
        </div>
        <div class="col-lg-3 col-md-6">
            <input type="number" class="form-control quantity" name="quantity[]" placeholder="Quantity" value="1" min="1" onchange="updatePrice(this)">
        </div>
        <div class="col-lg-2 col-md-6">
            <input type="text" class="form-control price" name="price[]" placeholder="Price" readonly>
        </div>
        <div class="col-lg-2 col-md-6 d-flex align-items-center">
            <button type="button" class="btn btn-custom btn-success" onclick="addItem()">+</button>
            <button type="button" class="btn btn-custom btn-danger ms-2" onclick="removeItem(this)" style="display:none;">-</button>
        </div>
    `;
    orderItems.appendChild(newItem);
    updateSelectors();
    updateButtons();
    updateTotal();
}

function removeItem(button) {
    const row = button.closest('.order-item');
    row.remove();
    updateButtons();
    updateTotal();
}

function updateButtons() {
    const items = document.querySelectorAll('.order-item');
    items.forEach((item, index) => {
        const addButton = item.querySelector('.btn-success');
        const removeButton = item.querySelector('.btn-danger');
        addButton.style.display = index === items.length - 1 ? 'inline-block' : 'none';
        removeButton.style.display = items.length > 1 ? 'inline-block' : 'none';
    });
}

//async function getAuthToken() {
//    fetch('https://localhost:7135/api/Login/GetAuthToken', {
//        method: 'GET',
//        headers: {
//            'Content-Type': 'application/json'
//        },
//        credentials: 'include' // Đảm bảo rằng cookie được gửi cùng với yêu cầu
//    })
//        .then(response => response.json())
//        .then(data => {
//            console.log("Auth token:", data); // Assuming `data` contains the auth token
//        })
//        .catch(error => console.error('Error:', error));

//}

function updateTotal() {
    let total = 0;
    document.querySelectorAll('.order-item').forEach(item => {
        const priceValue = item.querySelector('.price').value;
        if (priceValue) {
            const price = parseFloat(priceValue.replace('$', '')) || 0;
            total += price;
        }
    });
    document.getElementById('total').textContent = `Total: ${total.toFixed(2)} $`;
}

document.addEventListener('DOMContentLoaded', function () {
    fetchProducts();
    updateButtons();
    updateTotal();

    const formElement = document.getElementById('orderForm');
    if (formElement) {
        formElement.addEventListener('submit', async function (event) {
            event.preventDefault();

            const orderDetails = [];
            let total = 0;

            document.querySelectorAll('.order-item').forEach(item => {
                const productId = item.querySelector('.item-select').value;
                const quantity = item.querySelector('.quantity').value;
                const price = item.querySelector('.item-select option:checked').getAttribute('data-price');

                if (productId && quantity && price) {
                    const itemTotal = price * parseInt(quantity);

                    orderDetails.push({
                        productId: parseInt(productId),
                        quantity: parseInt(quantity),
                        price: parseFloat(itemTotal),
                    });

                    total += itemTotal;
                }
            });

            const addressCus = document.getElementById('tableDropdown').value;
            const phoneCus = document.getElementById('phoneCus').value;
            const nameCus = document.getElementById('nameCus').value;
            const emailCus = document.getElementById('emailCus').value;

            const customer = {
                phone: phoneCus,
                email: emailCus,
                name: nameCus
            };

            const orderData = {
                freight: total,
                tableAdress: addressCus,
                customer: customer,
                orderDetails: orderDetails
            };

            try {
                const response = await fetch('https://localhost:7135/api/Order', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    credentials: 'include' ,// Đảm bảo rằng cookie được gửi cùng với yêu cầu
                    body: JSON.stringify(orderData)
                });
                console.log(JSON.stringify(orderData))
                if (!response.ok) {
                    const errorText = await response.text(); // Lấy nội dung lỗi từ phản hồi
                    throw new Error(`Network response was not ok: ${errorText}`);
                }

                /*const result = await response.json();*/
                console.log('Order Data:', orderData);
                alert('Order successfully created!');
            } catch (error) {
                alert("nosuccess" + error);
            }
        });
    } else {
        console.error('Form element with id "orderForm" not found.');
    }
});
