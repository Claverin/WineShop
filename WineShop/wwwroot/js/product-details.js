document.addEventListener("DOMContentLoaded", function () {
    const ratingInput = document.getElementById("Rating");

    if (ratingInput) {
        const setRating = parseInt(ratingInput.value || "0");

        for (let i = 1; i <= setRating; i++) {
            const star = document.getElementById("Rate" + i);
            if (star) {
                star.className = "fa-star fa-solid";
            }
        }
    }
});

function sendRating() {
    const form = document.getElementById("cartAndRatingForm");

    if (!form) {
        return;
    }

    const url = form.dataset.rateUrl;

    $.ajax({
        type: "POST",
        url: url,
        data: $(form).serialize()
    });
}

function CRate(r) {
    const ratingInput = document.getElementById("Rating");

    if (!ratingInput) {
        return;
    }

    if (parseInt(ratingInput.value) === r) {
        ratingInput.value = 0;

        for (let i = r + 1; i <= 5; i++) {
            const star = document.getElementById("Rate" + i);
            if (star) {
                star.className = "fa-star fa-regular";
            }
        }

        sendRating();
    } else {
        ratingInput.value = r;

        for (let i = 1; i <= r; i++) {
            const star = document.getElementById("Rate" + i);
            if (star) {
                star.className = "fa-star fa-solid";
            }
        }

        for (let i = r + 1; i <= 5; i++) {
            const star = document.getElementById("Rate" + i);
            if (star) {
                star.className = "fa-star fa-regular";
            }
        }

        sendRating();
    }
}

function CRateOver(r) {
    for (let i = 1; i <= r; i++) {
        const star = document.getElementById("Rate" + i);
        if (star) {
            star.className = "fa-star fa-solid";
        }
    }
}

function CRateOut(r) {
    for (let i = 1; i <= r; i++) {
        const star = document.getElementById("Rate" + i);
        if (star) {
            star.className = "fa-star fa-regular";
        }
    }
}

function CRateSelected() {
    const ratingInput = document.getElementById("Rating");

    if (!ratingInput) {
        return;
    }

    const setRating = parseInt(ratingInput.value || "0");

    for (let i = 1; i <= setRating; i++) {
        const star = document.getElementById("Rate" + i);
        if (star) {
            star.className = "fa-star fa-solid";
        }
    }
}

function changeQuantity(step) {
    const input = document.getElementById("quantityInput");

    if (!input) {
        return;
    }

    let value = parseInt(input.value || "1");

    if (isNaN(value) || value < 1) {
        value = 1;
    }

    value += step;

    if (value < 1) {
        value = 1;
    }

    input.value = value;
}

window.sendRating = sendRating;
window.CRate = CRate;
window.CRateOver = CRateOver;
window.CRateOut = CRateOut;
window.CRateSelected = CRateSelected;
window.changeQuantity = changeQuantity;