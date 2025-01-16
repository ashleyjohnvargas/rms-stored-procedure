// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function to open a specific pop-up
function openPopup(popupId) {
    document.getElementById(popupId).style.display = 'block';
}

// Function to close a specific pop-up
function closePopup(popupId) {
    document.getElementById(popupId).style.display = 'none';
}


// Function to display a success message
function showMessage(message) {
    // Set the message text
    document.getElementById('message-text').textContent = message;

    // Display the message box
    document.getElementById('message-box').style.display = 'block';
}

// Function to close the message box
function closeMessageBox() {
    document.getElementById('message-box').style.display = 'none';
}

// Example usage: Call this function after submitting the form
function handlePayRent() {
    // Simulate a successful payment action
    closePopup('pay-rent-popup'); // Close the Pay Rent popup
    showMessage('You have successfully paid your rent.');
}

function handleTerminateLease() {
    // Simulate a successful lease termination action
    closePopup('terminate-lease-popup'); // Close the Terminate Lease popup
    showMessage('Your lease has been successfully terminated.');
}
