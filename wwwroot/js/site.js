document.addEventListener("DOMContentLoaded", function () {
    setTimeout(() => {
        document.querySelectorAll(".card").forEach((card, index) => {
            setTimeout(() => {
                card.classList.add("show");
            }, index * 100);
        });
    }, 200);

    document.querySelectorAll(".nav-link").forEach((link) => {
        link.addEventListener("mouseenter", function () {
            this.classList.add("pulse");
        });

        link.addEventListener("mouseleave", function () {
            this.classList.remove("pulse");
        });
    });

    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener("click", function (e) {
            e.preventDefault();

            const target = document.querySelector(this.getAttribute("href"));
            if (target) {
                target.scrollIntoView({
                    behavior: "smooth",
                });
            }
        });
    });

    window.showToast = function (message, type = "info", duration = 3000) {
        const toast = document.createElement("div");
        toast.className = `toast toast-${type} show`;
        toast.innerHTML = `<div class="toast-content"><i class="fas fa-${getIconForType(
            type
        )}"></i>${message}</div>`;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.classList.add("hide");
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, duration);
    };

    function getIconForType(type) {
        switch (type) {
            case "success":
                return "check-circle";
            case "error":
                return "exclamation-circle";
            case "warning":
                return "exclamation-triangle";
            default:
                return "info-circle";
        }
    }

    const style = document.createElement("style");
    style.textContent = `
        .toast {
            position: fixed;
            top: 20px;
            right: 20px;
            min-width: 250px;
            padding: 15px 20px;
            border-radius: 4px;
            color: white;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            z-index: 9999;
            animation: slideIn 0.3s ease-in-out;
            display: flex;
            align-items: center;
        }
        
        .toast.hide {
            animation: slideOut 0.3s ease-in-out forwards;
        }
        
        @keyframes slideIn {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        
        @keyframes slideOut {
            from { transform: translateX(0); opacity: 1; }
            to { transform: translateX(100%); opacity: 0; }
        }
        
        .toast-content {
            display: flex;
            align-items: center;
        }
        
        .toast-content i {
            margin-right: 10px;
        }
        
        .toast-info {
            background-color: #17a2b8;
        }
        
        .toast-success {
            background-color: #28a745;
        }
        
        .toast-warning {
            background-color: #ffc107;
            color: #343a40;
        }
        
        .toast-error {
            background-color: #dc3545;
        }
        
        .pulse {
            animation: pulse 0.5s ease-in-out;
        }
        
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.05); }
            100% { transform: scale(1); }
        }
        
        .card {
            opacity: 0;
            transform: translateY(20px);
            transition: all 0.4s ease-in-out;
        }
        
        .card.show {
            opacity: 1;
            transform: translateY(0);
        }
    `;

    document.head.appendChild(style);
});
