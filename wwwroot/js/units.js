// JavaScript for Search, Pagination, and Actions on Units List

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.querySelector("#searchInput");
    const tableRows = document.querySelectorAll(".units-table tbody tr");

    // Search functionality
    searchInput.addEventListener("keyup", function () {
        const filter = searchInput.value.toLowerCase();
        tableRows.forEach(row => {
            const unitName = row.querySelector("td:nth-child(2)").textContent.toLowerCase();
            if (unitName.includes(filter)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    });

    // Pagination functionality (example: 5 rows per page)
    const rowsPerPage = 5;
    const paginationControls = document.querySelector(".pagination-controls");
    let currentPage = 1;

    function showPage(pageNumber) {
        tableRows.forEach((row, index) => {
            if (index >= (pageNumber - 1) * rowsPerPage && index < pageNumber * rowsPerPage) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    }

    function setupPagination() {
        const totalPages = Math.ceil(tableRows.length / rowsPerPage);
        paginationControls.innerHTML = "";

        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement("button");
            pageButton.textContent = i;
            pageButton.classList.add("page-btn");
            if (i === currentPage) pageButton.classList.add("active");

            pageButton.addEventListener("click", () => {
                currentPage = i;
                document.querySelector(".page-btn.active").classList.remove("active");
                pageButton.classList.add("active");
                showPage(currentPage);
            });

            paginationControls.appendChild(pageButton);
        }
    }

    // Initial setup
    showPage(currentPage);
    setupPagination();

    // Actions (Edit and Delete functionality)
    document.querySelectorAll(".btn-edit").forEach(button => {
        button.addEventListener("click", function () {
            const unitName = this.closest("tr").querySelector("td:nth-child(2)").textContent;
            alert(`Edit functionality for: ${unitName}`);
            // Implement edit logic here
        });
    });

    document.querySelectorAll(".btn-delete").forEach(button => {
        button.addEventListener("click", function () {
            const unitName = this.closest("tr").querySelector("td:nth-child(2)").textContent;
            const confirmDelete = confirm(`Are you sure you want to delete ${unitName}?`);
            if (confirmDelete) {
                this.closest("tr").remove();
                setupPagination();
                showPage(currentPage);
            }
        });
    });
});
