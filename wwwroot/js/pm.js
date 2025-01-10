// JavaScript for Dashboard Charts and Interactions

// Example Data for Charts
const paymentStatusData = {
    labels: ["Unpaid", "Paid", "Overdue"],
    datasets: [
        {
            label: "Payment Status",
            data: [30, 50, 20],
            backgroundColor: ["#ffcc00", "#4caf50", "#f44336"],
        },
    ],
};

const maintenanceStatusData = {
    labels: ["Completed", "Pending", "In Progress"],
    datasets: [
        {
            label: "Maintenance Status",
            data: [45, 35, 20],
            backgroundColor: ["#4caf50", "#f44336", "#ff9800"],
        },
    ],
};

const topRentedUnitsData = {
    labels: ["Cozy Suites", "Cozy Rooms", "Cozy Deluxe"],
    datasets: [
        {
            label: "Top Rented Units",
            data: [40, 30, 30],
            backgroundColor: ["#8e44ad", "#3498db", "#e67e22"],
        },
    ],
};

// Render Charts
function renderCharts() {
    const paymentStatusCtx = document.getElementById("paymentStatusChart").getContext("2d");
    new Chart(paymentStatusCtx, {
        type: "bar",
        data: paymentStatusData,
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: "top",
                },
            },
        },
    });

    const maintenanceStatusCtx = document.getElementById("maintenanceStatusChart").getContext("2d");
    new Chart(maintenanceStatusCtx, {
        type: "pie",
        data: maintenanceStatusData,
        options: {
            responsive: true,
        },
    });

    const topRentedUnitsCtx = document.getElementById("topRentedUnitsChart").getContext("2d");
    new Chart(topRentedUnitsCtx, {
        type: "bar",
        data: topRentedUnitsData,
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: false,
                },
            },
        },
    });
}

// Initialize Charts on DOM Load
document.addEventListener("DOMContentLoaded", renderCharts);
