/**
 * News Page JavaScript Functionality
 * Handles news page interactions and enhancements
 */

(function () {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeNewsPage();
    });

    /**
     * Initialize news page functionality
     */
    function initializeNewsPage() {
        initializeNewsCards();
        initializeReadMore();
        initializeShareFunctionality();
        initializeFavorites();
        initializeSearch();
        initializeLazyLoading();
    }

    /**
     * Enhanced news card interactions
     */
    function initializeNewsCards() {
        const newsCards = document.querySelectorAll('.news-item');

        newsCards.forEach(card => {
            // Add hover sound effect (optional)
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-8px) scale(1.02)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0) scale(1)';
            });

            // Add click analytics (placeholder)
            card.addEventListener('click', function (e) {
                if (!e.target.closest('.news-actions')) {
                    trackNewsClick(this);
                }
            });
        });
    }

    /**
     * Enhanced Read More functionality
     */
    function initializeReadMore() {
        const readMoreButtons = document.querySelectorAll('.read-more');

        readMoreButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const newsItem = this.closest('.news-item');
                const excerpt = newsItem.querySelector('.news-excerpt');
                const newsId = newsItem.getAttribute('data-news-id');

                // Add loading state
                this.innerHTML = '<i class="bi bi-hourglass-split"></i> Loading...';
                this.classList.add('disabled');

                // Simulate loading (replace with actual AJAX call)
                setTimeout(() => {
                    // Reset button
                    this.innerHTML = 'Read More <i class="bi bi-arrow-right"></i>';
                    this.classList.remove('disabled');

                    // You can implement modal or navigation here
                    openNewsModal(newsId);
                }, 500);
            });
        });
    }

    /**
     * Share functionality
     */
    function initializeShareFunctionality() {
        const shareButtons = document.querySelectorAll('.news-actions a[title*="share"], .news-actions a:last-child');

        shareButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const newsItem = this.closest('.news-item');
                const title = newsItem.querySelector('.news-title a').textContent;
                const url = window.location.href;

                if (navigator.share) {
                    navigator.share({
                        title: title,
                        url: url
                    });
                } else {
                    copyToClipboard(url);
                    showToast('Link copied to clipboard!');
                }
            });
        });
    }

    /**
     * Favorites functionality
     */
    function initializeFavorites() {
        const favoriteButtons = document.querySelectorAll('.news-actions a[title*="favorite"], .news-actions a:first-child');

        favoriteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const newsItem = this.closest('.news-item');
                const newsId = newsItem.getAttribute('data-news-id');
                const icon = this.querySelector('i');

                // Toggle favorite state
                if (icon.classList.contains('bi-heart')) {
                    icon.classList.remove('bi-heart');
                    icon.classList.add('bi-heart-fill');
                    this.style.color = '#dc3545';
                    addToFavorites(newsId);
                    showToast('Added to favorites!');
                } else {
                    icon.classList.remove('bi-heart-fill');
                    icon.classList.add('bi-heart');
                    this.style.color = '';
                    removeFromFavorites(newsId);
                    showToast('Removed from favorites!');
                }
            });
        });
    }

    /**
     * Search functionality (if implemented)
     */
    function initializeSearch() {
        const searchInput = document.querySelector('#news-search');
        if (searchInput) {
            let searchTimeout;

            searchInput.addEventListener('input', function () {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    filterNews(this.value);
                }, 300);
            });
        }
    }

    /**
     * Lazy loading for images
     */
    function initializeLazyLoading() {
        const images = document.querySelectorAll('.news-img img');

        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.classList.add('loaded');
                        observer.unobserve(img);
                    }
                });
            });

            images.forEach(img => imageObserver.observe(img));
        }
    }

    /**
     * Utility Functions
     */

    function trackNewsClick(newsItem) {
        // Implement analytics tracking
        console.log('News item clicked:', newsItem.querySelector('.news-title').textContent);
    }

    function openNewsModal(newsId) {
        // Implement modal or navigation to full article
        console.log('Opening news article:', newsId);
        // Example: window.location.href = `/News/Details/${newsId}`;
    }

    function addToFavorites(newsId) {
        // Implement add to favorites
        let favorites = JSON.parse(localStorage.getItem('news-favorites') || '[]');
        if (!favorites.includes(newsId)) {
            favorites.push(newsId);
            localStorage.setItem('news-favorites', JSON.stringify(favorites));
        }
    }

    function removeFromFavorites(newsId) {
        // Implement remove from favorites
        let favorites = JSON.parse(localStorage.getItem('news-favorites') || '[]');
        favorites = favorites.filter(id => id !== newsId);
        localStorage.setItem('news-favorites', JSON.stringify(favorites));
    }

    function copyToClipboard(text) {
        navigator.clipboard.writeText(text).catch(() => {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
        });
    }

    function showToast(message, type = 'success') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : '#dc3545'};
            color: white;
            padding: 12px 20px;
            border-radius: 8px;
            z-index: 9999;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;

        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        // Remove after 3 seconds
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, 3000);
    }

    function filterNews(searchTerm) {
        const newsItems = document.querySelectorAll('.news-item');
        const term = searchTerm.toLowerCase().trim();

        newsItems.forEach(item => {
            const title = item.querySelector('.news-title').textContent.toLowerCase();
            const excerpt = item.querySelector('.news-excerpt').textContent.toLowerCase();
            const category = item.querySelector('.news-category-badge').textContent.toLowerCase();

            if (title.includes(term) || excerpt.includes(term) || category.includes(term)) {
                item.style.display = '';
                item.style.animation = 'fadeInUp 0.5s ease';
            } else {
                item.style.display = 'none';
            }
        });

        // Update results count
        const visibleItems = document.querySelectorAll('.news-item:not([style*="display: none"])');
        updateResultsCount(visibleItems.length);
    }

    function updateResultsCount(count) {
        let resultsCounter = document.querySelector('.search-results-count');
        if (!resultsCounter) {
            resultsCounter = document.createElement('div');
            resultsCounter.className = 'search-results-count';
            resultsCounter.style.cssText = `
                margin-bottom: 20px;
                padding: 10px 15px;
                background: #f8f9fa;
                border-radius: 8px;
                font-weight: 600;
                color: #495057;
            `;
            document.querySelector('.row.gy-4').insertAdjacentElement('beforebegin', resultsCounter);
        }

        resultsCounter.textContent = `${count} article${count !== 1 ? 's' : ''} found`;
        resultsCounter.style.display = count === document.querySelectorAll('.news-item').length ? 'none' : 'block';
    }

    // Load favorites on page load
    function loadFavorites() {
        const favorites = JSON.parse(localStorage.getItem('news-favorites') || '[]');
        const favoriteButtons = document.querySelectorAll('.news-actions a:first-child');

        favoriteButtons.forEach(button => {
            const newsItem = button.closest('.news-item');
            const newsId = newsItem.getAttribute('data-news-id');

            if (favorites.includes(newsId)) {
                const icon = button.querySelector('i');
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                button.style.color = '#dc3545';
            }
        });
    }

    // Initialize favorites on load
    document.addEventListener('DOMContentLoaded', loadFavorites);

    // Smooth scroll for pagination
    function initializePagination() {
        const paginationLinks = document.querySelectorAll('.pagination .page-link');

        paginationLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                if (!this.closest('.page-item').classList.contains('disabled')) {
                    // Add loading state
                    this.innerHTML = '<i class="bi bi-hourglass-split"></i>';

                    // Scroll to top of news section
                    document.querySelector('#news').scrollIntoView({
                        behavior: 'smooth'
                    });
                }
            });
        });
    }

    // Initialize pagination
    document.addEventListener('DOMContentLoaded', initializePagination);
})();