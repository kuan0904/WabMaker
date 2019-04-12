
/*=================================================================
             UTRUST JS - All Rights Reserved.
==================================================================*/
// Slider
if ( $('#Slider').length )
{
    var swiper = new Swiper('#Slider', {
        pagination: '.swiper-pagination',
        nextButton: '.swiper-button-next',
        prevButton: '.swiper-button-prev',
        preventClicks: false,
        preventClicksPropagation: false,
        slidesPerView: 1,
        paginationClickable: true,
        spaceBetween: 0,
        loop: false
    });
};

//slider news
if ( $('#news-sldier').length ){
    var swiper = new Swiper('#news-sldier', {
        pagination: '.swiper-pagination',
        nextButton: '.swiper-button-next',
        prevButton: '.swiper-button-prev',
        preventClicks: false,
        preventClicksPropagation: false,
        slidesPerView: 1,
        paginationClickable: true,
        spaceBetween: 0,
        loop: false
    });

};


//news2-sldier
if ( $('#news2-sldier').length ){
    var swiper = new Swiper('#news2-sldier', {
      slidesPerView: 5,
      spaceBetween: 1,
      pagination: '.swiper-pagination',
      breakpoints: {
        320: {
          slidesPerView: 1,
          spaceBetween: 0
        },
        640: {
          slidesPerView: 2,
          spaceBetween: 1
        },
        991: {
          slidesPerView: 3,
          spaceBetween: 1
        },
        1024: {
          slidesPerView: 4,
          spaceBetween: 1
        }
      }
    });
};

//youtube popup
if ( $('.popup-youtube').length ){
  $('.popup-youtube, .popup-vimeo, .popup-gmaps').magnificPopup({
        disableOn: 700,
        type: 'iframe',
        mainClass: 'mfp-fade',
        removalDelay: 160,
        // preloader: false,
        // fixedContentPos: false
   });
}

//img-cover
function addClass(el, className) {
    if (el.classList)
        el.classList.add(className);
    else if (!hasClass(el, className))
        el.className += ' ' + className;
}
//var imgContainers, len;
//if (!Modernizr.objectfit) {
//    imgContainers = document.querySelectorAll('.pic');
//    len = imgContainers.length;
//    for (var i = 0; i < len; i++) {
//        if (window.CP.shouldStopExecution(1)) {
//            break;
//        }
//        var $container = imgContainers[i], imgUrl = $container.querySelector('img').getAttribute('src');
//        if (imgUrl) {
//            $container.style.backgroundImage = 'url(' + imgUrl + ')';
//            addClass($container, 'compat-object-fit');
//        }
//    }
//    window.CP.exitedLoop(1);
//}
