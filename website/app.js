if (window.lucide) {
  lucide.createIcons();
}

const menuButton = document.querySelector("#menuButton");
const mobileMenu = document.querySelector("#mobileMenu");

menuButton?.addEventListener("click", () => {
  const isOpen = !mobileMenu.classList.contains("hidden");
  mobileMenu.classList.toggle("hidden", isOpen);
  menuButton.setAttribute("aria-expanded", String(!isOpen));
});

mobileMenu?.querySelectorAll("a").forEach((link) => {
  link.addEventListener("click", () => {
    mobileMenu.classList.add("hidden");
    menuButton?.setAttribute("aria-expanded", "false");
  });
});

const goTopButton = document.querySelector("#goTopButton");

const toggleGoTopButton = () => {
  goTopButton?.classList.toggle("is-visible", window.scrollY > 420);
};

window.addEventListener("scroll", toggleGoTopButton, { passive: true });
toggleGoTopButton();

goTopButton?.addEventListener("click", () => {
  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
});

let workflowSwiper;

if (window.Swiper) {
  workflowSwiper = new Swiper(".workflow-slider", {
    slidesPerView: 1,
    spaceBetween: 16,
    loop: true,
    speed: 650,
    grabCursor: true,
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-next",
      prevEl: ".swiper-prev",
    },
    autoplay: {
      delay: 3800,
      disableOnInteraction: false,
    },
    breakpoints: {
      720: {
        slidesPerView: 2,
      },
      1080: {
        slidesPerView: 3,
      },
    },
  });
}

document.addEventListener("visibilitychange", () => {
  if (!workflowSwiper?.autoplay) {
    return;
  }

  if (document.hidden) {
    workflowSwiper.autoplay.stop();
  } else {
    workflowSwiper.autoplay.start();
  }
});
