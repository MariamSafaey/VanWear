// checkoutform.js

document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const name = document.querySelector("input[name='Name']");
    const email = document.querySelector("input[name='Email']");
    const mobile = document.querySelector("input[name='MobileNumber']");
    const address = document.querySelector("textarea[name='Address']");
    const payment = document.querySelector("select[name='PaymentMethod']");
    const successMessage = document.getElementById("successMessage");

    // ========= Fade-in Animation =========
    const fields = document.querySelectorAll("input, textarea, select, button");
    fields.forEach((el, index) => {
        el.style.opacity = 0;
        setTimeout(() => {
            el.style.transition = "opacity 0.5s ease-in-out";
            el.style.opacity = 1;
        }, index * 200);
    });

    // ========= Field Focus/Blur Effects =========
    document.querySelectorAll("input, textarea, select").forEach(field => {
        field.addEventListener("focus", () => {
            field.style.backgroundColor = "#f0f8ff";
        });

        field.addEventListener("blur", () => {
            field.style.backgroundColor = "";
            if (field.value.trim()) {
                field.style.borderColor = "green";
            } else {
                field.style.borderColor = "red";
            }
        });
    });

    // ========= Form Validation =========
    form.addEventListener("submit", function (e) {
        let isValid = true;
        clearErrors();

        // Name validation
        if (!name.value.trim()) {
            showError(name, "Name is required.");
            isValid = false;
        }

        // Email validation
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!email.value.trim()) {
            showError(email, "Email is required.");
            isValid = false;
        } else if (!emailPattern.test(email.value)) {
            showError(email, "Please enter a valid email.");
            isValid = false;
        }

        // Mobile validation
        const phonePattern = /^[0-9]{10,15}$/;
        if (!mobile.value.trim()) {
            showError(mobile, "Mobile number is required.");
            isValid = false;
        } else if (!phonePattern.test(mobile.value)) {
            showError(mobile, "Enter a valid phone number.");
            isValid = false;
        }

        // Address
        if (!address.value.trim()) {
            showError(address, "Address is required.");
            isValid = false;
        }

        // Payment
        if (!payment.value) {
            showError(payment, "Please select a payment method.");
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        } else {
            successMessage.style.display = "block";
            successMessage.scrollIntoView({ behavior: "smooth" });
        }
    });

    function showError(element, message) {
        const errorSpan = document.createElement("span");
        errorSpan.classList.add("text-danger", "d-block", "mt-1");
        errorSpan.innerText = message;
        element.parentElement.appendChild(errorSpan);
    }

    function clearErrors() {
        const errors = document.querySelectorAll(".text-danger.d-block");
        errors.forEach(error => error.remove());
    }
});
