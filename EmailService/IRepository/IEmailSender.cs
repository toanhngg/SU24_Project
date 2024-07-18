﻿namespace EmailService.IRepository
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

}
