<script>
    function openModal() {
        document.querySelector('.modal').style.display = 'flex';
    }

    function closeModal() {
        document.querySelector('.modal').style.display = 'none';
    }

    document.addEventListener('DOMContentLoaded', () => {
        const form = document.getElementById('addUnitForm');
        form.addEventListener('submit', (e) => {
        // Optional: Validate form data before submission
        console.log('Form submitted');
        });
    });
</script>
