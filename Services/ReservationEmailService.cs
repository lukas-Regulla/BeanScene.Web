using BeanScene.Web.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BeanScene.Web.Services;

public class ReservationEmailService : IReservationEmailService
{
    private readonly IEmailSender _emailSender;

    public ReservationEmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendCreatedAsync(Reservation reservation, SittingSchedule sitting)
    {
        if (string.IsNullOrEmpty(reservation.Email)) return;

        var subject = "Reservation Created";
        var message = $"Dear {reservation.FirstName} {reservation.LastName},<br/><br/>" +
                      $"Your reservation for {reservation.NumOfGuests} guests on {sitting.Stype} sitting " +
                      $"at {reservation.StartTime:h:mm tt} has been created successfully.<br/><br/>" +
                      "Thank you for choosing our restaurant!<br/><br/>" +
                      "Best regards,<br/>BeanScene Team";

        await _emailSender.SendEmailAsync(reservation.Email, subject, message);
    }

    public async Task SendConfirmedAsync(Reservation reservation)
    {
        if (string.IsNullOrEmpty(reservation.Email)) return;

        var subject = "Your Reservation is Confirmed";
        var message = $@"
                <h2>Your Reservation is Confirmed!</h2>
                <p>Hi {reservation.FirstName},</p>
                <p>Your reservation at BeanScene Café has been confirmed.</p>

                <h3>Reservation Details</h3>
                <p><strong>Date:</strong> {reservation.StartTime:dddd, dd MMM yyyy}</p>
                <p><strong>Start Time:</strong> {reservation.StartTime:hh:mm tt}</p>
                <p><strong>Guests:</strong> {reservation.NumOfGuests}</p>
                <p><strong>Duration:</strong> {reservation.Duration} minutes</p>
                <p><strong>Status:</strong> Confirmed</p>

                <p>We look forward to seeing you!</p>
            ";

        await _emailSender.SendEmailAsync(reservation.Email, subject, message);
    }
}
