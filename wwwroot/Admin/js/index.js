// =======================================================
// TRIBEAN ADMIN - TỔNG HỢP LOGIC JAVASCRIPT
// =======================================================

document.addEventListener("DOMContentLoaded", function () {

    /* ── 1. LOGIC DROPDOWN AVATAR ──────────────────────── */
    const profileBtn = document.getElementById("profileDropdownBtn");
    const profileMenu = document.getElementById("profileDropdownMenu");
    
    if (profileBtn && profileMenu) {
        profileBtn.addEventListener("click", function (e) {
            e.stopPropagation(); 
            profileMenu.classList.toggle("show");
        });

        // Click ra ngoài khoảng trống thì tự động đóng menu
        document.addEventListener("click", function (e) {
            if (!profileMenu.contains(e.target)) {
                profileMenu.classList.remove("show");
            }
        });
    }

    /* ── 2. LOGIC ĐỔI THEME (DARK/LIGHT MODE) ──────────── */
    const themeToggleBtn = document.getElementById('themeToggleBtn');
    const moonIcon = document.getElementById('moonIcon');
    const sunIcon = document.getElementById('sunIcon');
    const themeText = document.getElementById('themeText'); 

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation(); // Giữ menu mở khi click đổi màu

            let currentTheme = document.documentElement.getAttribute('data-theme');
            let targetTheme = currentTheme === 'dark' ? 'light' : 'dark';

            // Cập nhật DOM và lưu trữ
            document.documentElement.setAttribute('data-theme', targetTheme);
            localStorage.setItem('adminTheme', targetTheme);

            updateThemeUI(targetTheme);
        });
    }

    // Hàm cập nhật Icon và Chữ cho Theme
    function updateThemeUI(theme) {
        if (!moonIcon || !sunIcon) return; // Nếu giao diện không có text thì chỉ đổi icon
        
        if (theme === 'dark') {
            moonIcon.style.display = 'none';
            sunIcon.style.display = 'block';
            if (themeText) themeText.innerText = 'Light Mode';
        } else {
            moonIcon.style.display = 'block';
            sunIcon.style.display = 'none';
            if (themeText) themeText.innerText = 'Dark Mode';
        }
    }

    // Load chuẩn xác icon sau khi khởi động
    updateThemeUI(document.documentElement.getAttribute('data-theme') || 'light');


    /* ── 3. ĐIỀU KHIỂN SIDEBAR & MENU ACTIVE ────────────── */
    const sidebar   = document.getElementById('sidebar');
    const overlay   = document.getElementById('sidebarOverlay');
    const toggleBtn = document.getElementById('sidebarToggle');

    if (toggleBtn && sidebar && overlay) {
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('open');
            overlay.style.display = sidebar.classList.contains('open') ? 'block' : 'none';
        });
    }

    if (overlay && sidebar) {
        overlay.addEventListener('click', () => {
            sidebar.classList.remove('open');
            overlay.style.display = 'none';
        });
    }

    // Active thẻ menu hiện tại dựa theo URL
    const navLinks = document.querySelectorAll('.nav-item[href]');
    navLinks.forEach(link => {
        if (link.getAttribute('href') === window.location.pathname) {
            link.classList.add('active');
        }
    });


    /* ── 4. REALTIME BÁO ĐƠN HÀNG MỚI (POLLING) ─────────── */
    const pendingOrderBadge = document.getElementById('pendingOrderBadge');
    if (pendingOrderBadge) {
        setInterval(function () {
            fetch('/Admin/Order/GetPendingCount')
                .then(response => response.json())
                .then(data => {
                    if (data.count > 0) {
                        pendingOrderBadge.innerText = data.count;
                        pendingOrderBadge.style.display = 'inline-flex';
                    } else {
                        pendingOrderBadge.style.display = 'none';
                    }
                })
                .catch(error => console.error('Lỗi khi check đơn mới:', error));
        }, 3000); // Check mỗi 3 giây
    }


    /* ── 5. TIM KIẾM NHANH (DEBOUNCE SEARCH) ───────────── */
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        let debounceTimer;
        searchInput.addEventListener("input", () => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                searchInput.closest("form")?.submit();
            }, 500); // Dừng gõ 0.5s mới submit form
        });
    }


    /* ── 6. CHECKBOX "CHỌN TẤT CẢ" ─────────────────────── */
    const checkAll = document.getElementById("checkAll");
    if (checkAll) {
        checkAll.addEventListener("change", () => {
            document.querySelectorAll(".row-check").forEach((cb) => {
                cb.checked = checkAll.checked;
            });
        });
    }


    /* ── 7. KHÔI PHỤC CHẾ ĐỘ XEM SẢN PHẨM (VIEW TOGGLE) ── */
    const savedView = localStorage.getItem("productView") || "table";
    if (document.getElementById("tableViewBtn")) {
        setView(savedView);
    }
});


// =======================================================
// CÁC HÀM TIỆN ÍCH DÙNG CHUNG (GLOBAL FUNCTIONS)
// (Gọi từ các sự kiện onclick trên HTML)
// =======================================================

/* ── Chuyển đổi Table / Grid (Sản phẩm) ── */
function setView(type) {
    const tableView = document.getElementById("tableView");
    const gridView = document.getElementById("gridView");
    const tableBtn = document.getElementById("tableViewBtn");
    const gridBtn = document.getElementById("gridViewBtn");
    const pagination = document.getElementById("paginationControls");

    if (!tableView || !gridView) return; 

    if (type === "table") {
        tableView.style.display = "block";
        gridView.style.display = "none";
        if (pagination) pagination.style.display = "flex";
        
        if (tableBtn) tableBtn.classList.add("active");
        if (gridBtn) gridBtn.classList.remove("active");
        
        localStorage.setItem("productView", "table");
    } else {
        tableView.style.display = "none";
        gridView.style.display = "block";
        if (pagination) pagination.style.display = "none";
        
        if (gridBtn) gridBtn.classList.add("active");
        if (tableBtn) tableBtn.classList.remove("active");
        
        localStorage.setItem("productView", "grid");
    }
}

/* ── Xem trước ảnh khi Upload ── */
function previewImage(input) {
    if (!input.files || !input.files[0]) return;

    const reader = new FileReader();
    reader.onload = (e) => {
        const preview = document.getElementById("imgPreview");
        const placeholder = document.getElementById("uploadPlaceholder");

        if (preview) {
            preview.src = e.target.result;
            preview.classList.remove("d-none");
            preview.style.display = "block";
        }
        if (placeholder) {
            placeholder.classList.add("d-none");
            placeholder.style.display = "none";
        }
    };
    reader.readAsDataURL(input.files[0]);
}

/* ── Toggle trạng thái (Active/Inactive) qua AJAX ── */
function toggleStatus(productId, element) {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert("Lỗi bảo mật: Không tìm thấy khóa xác thực (AntiForgeryToken).");
        return;
    }

    fetch(`/Admin/Product/ToggleStatus/${productId}`, {
        method: "POST",
        headers: {
            "RequestVerificationToken": tokenInput.value,
            "Content-Type": "application/json",
        },
    })
    .then((response) => response.json())
    .then((data) => {
        if (data.success) {
            if (data.newStatus === true) {
                element.className = "badge badge-green toggle-status-btn";
                element.innerText = "Active";
            } else {
                element.className = "badge badge-red toggle-status-btn";
                element.innerText = "Inactive";
            }
        } else {
            alert("Có lỗi xảy ra: " + data.message);
        }
    })
    .catch((error) => {
        console.error("Lỗi:", error);
        alert("Không thể kết nối đến máy chủ.");
    });
}

/* ── Modal Đánh giá (Review) ── */
function openReviewModal(customer, product, rating, comment) {
    document.getElementById("mdlCustomer").innerText = customer || "Unknown";
    document.getElementById("mdlProduct").innerText = product || "Deleted Product";
    document.getElementById("mdlComment").innerText = '"' + comment + '"';

    let starsHtml = "";
    for (let i = 1; i <= 5; i++) {
        starsHtml += i <= rating
            ? '<span class="rev-star-filled">★</span>'
            : '<span class="rev-star-empty">★</span>';
    }
    
    document.getElementById("mdlRating").innerHTML = starsHtml;
    document.getElementById("reviewDetailModal").classList.add("show");
}

function closeReviewModal() {
    const modal = document.getElementById("reviewDetailModal");
    if (modal) {
        modal.classList.remove("show");
    }
}

// Click ra ngoài vùng đen để đóng Modal
window.onclick = function (event) {
    const modal = document.getElementById("reviewDetailModal");
    if (event.target === modal) {
        closeReviewModal();
    }
};