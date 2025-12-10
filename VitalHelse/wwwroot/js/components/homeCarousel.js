
document.addEventListener('DOMContentLoaded', function() {
    const carousel = document.getElementById('homeBannerCarousel');
    if (!carousel) return;

    const track = carousel.querySelector('.hp-carousel-track');
    const slides = carousel.querySelectorAll('.hp-carousel-slide');
    const prevBtn = carousel.querySelector('.hp-carousel-control.hp-prev');
    const nextBtn = carousel.querySelector('.hp-carousel-control.hp-next');
    const indicators = carousel.querySelectorAll('.hp-indicator');
    
    const totalSlides = slides.length;
    let currentIndex = 0;
    let autoPlayInterval;
    let isTransitioning = false;

    const interval = parseInt(carousel.dataset.interval) || 6000;

    function showSlide(index) {
        if (isTransitioning) return;
        isTransitioning = true;

        currentIndex = index;
        const translateX = -(currentIndex * 100);
        track.style.transform = `translateX(${translateX}%)`;

        indicators.forEach((indicator, i) => {
            if (i === currentIndex) {
                indicator.classList.add('active');
            } else {
                indicator.classList.remove('active');
            }
        });

        setTimeout(() => {
            isTransitioning = false;
        }, 600); 
    }

    function nextSlide() {
        let next = currentIndex + 1;
        if (next >= totalSlides) {
            next = 0;
        }
        showSlide(next);
    }

    function prevSlide() {
        let prev = currentIndex - 1;
        if (prev < 0) {
            prev = totalSlides - 1;
        }
        showSlide(prev);
    }

    function startAutoPlay() {
        if (totalSlides <= 1) return;
        autoPlayInterval = setInterval(nextSlide, interval);
    }

    function stopAutoPlay() {
        clearInterval(autoPlayInterval);
    }

    if (prevBtn) {
        prevBtn.addEventListener('click', function() {
            stopAutoPlay();
            prevSlide();
            startAutoPlay();
        });
    }

    if (nextBtn) {
        nextBtn.addEventListener('click', function() {
            stopAutoPlay();
            nextSlide();
            startAutoPlay();
        });
    }

    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', function() {
            stopAutoPlay();
            showSlide(index);
            startAutoPlay();
        });
    });
    
    carousel.addEventListener('mouseenter', stopAutoPlay);
    carousel.addEventListener('mouseleave', startAutoPlay);

    let touchStartX = 0;
    let touchEndX = 0;

    carousel.addEventListener('touchstart', function(e) {
        touchStartX = e.changedTouches[0].screenX;
    }, { passive: true });

    carousel.addEventListener('touchend', function(e) {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }, { passive: true });

    function handleSwipe() {
        const swipeThreshold = 50;
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > swipeThreshold) {
            stopAutoPlay();
            if (diff > 0) {
                nextSlide();
            } else {
                prevSlide();
            }
            startAutoPlay();
        }
    }

    document.addEventListener('keydown', function(e) {
        if (e.key === 'ArrowLeft') {
            stopAutoPlay();
            prevSlide();
            startAutoPlay();
        } else if (e.key === 'ArrowRight') {
            stopAutoPlay();
            nextSlide();
            startAutoPlay();
        }
    });

    if (totalSlides > 1) {
        startAutoPlay();
    }
});
