document.addEventListener('DOMContentLoaded', function() {
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        const searchInput = searchForm.querySelector('input[name="search_query"]');
        searchInput.addEventListener('keypress', function(event) {
            if (event.key === 'Enter') {
                event.preventDefault(); // 기본 Enter 동작(줄바꿈 등) 방지
                searchForm.submit();
            }
        });
    }
});