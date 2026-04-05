// wwwroot/js/admin/index.js

/* ── View toggle (Table / Grid) ─────────────── */
function setView(type) {
  const tableView = document.getElementById("tableView");
  const gridView = document.getElementById("gridView");
  const tableBtn = document.getElementById("tableViewBtn");
  const gridBtn = document.getElementById("gridViewBtn");

  // Thẻ chứa phân trang mà chúng ta vừa đặt ID
  const pagination = document.getElementById("paginationControls");

  if (type === "table") {
    // Hiện Table, Ẩn Grid
    tableView.style.display = "block";
    gridView.style.display = "none";

    // Hiện thanh phân trang
    if (pagination) pagination.style.display = "flex";

    // Đổi trạng thái nút bấm (sáng/tối)
    tableBtn.classList.add("active");
    gridBtn.classList.remove("active");
  } else {
    // Hiện Grid, Ẩn Table
    tableView.style.display = "none";
    gridView.style.display = "block";

    // Ẩn thanh phân trang (Vì lưới đã hiện Full)
    if (pagination) pagination.style.display = "none";

    // Đổi trạng thái nút bấm
    gridBtn.classList.add("active");
    tableBtn.classList.remove("active");
  }
}

/* ── Restore last view from localStorage ─────── */
document.addEventListener("DOMContentLoaded", () => {
  const savedView = localStorage.getItem("productView") || "table";
  setView(savedView);
});

/* ── Search: submit form on Enter ────────────── */
document.addEventListener("DOMContentLoaded", () => {
  const searchInput = document.querySelector('input[name="search"]');
  if (!searchInput) return;

  let debounceTimer;
  searchInput.addEventListener("input", () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      searchInput.closest("form")?.submit();
    }, 500); // 500ms debounce
  });
});

/* ── Check all checkbox ──────────────────────── */
document.addEventListener("DOMContentLoaded", () => {
  const checkAll = document.getElementById("checkAll");
  if (!checkAll) return;

  checkAll.addEventListener("change", () => {
    document.querySelectorAll(".row-check").forEach((cb) => {
      cb.checked = checkAll.checked;
    });
  });
});

/* ── Image preview (Add/Edit page) ──────────── */
function previewImage(input) {
  if (!input.files || !input.files[0]) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    const preview = document.getElementById("imgPreview");
    const placeholder = document.getElementById("uploadPlaceholder");

    if (preview) {
      preview.src = e.target.result;
      preview.style.display = "block";
    }
    if (placeholder) {
      placeholder.style.display = "none";
    }
  };
  reader.readAsDataURL(input.files[0]);
}

/* ── Alert auto-hide ─────────────────────────── */
document.addEventListener("DOMContentLoaded", () => {
  const alerts = document.querySelectorAll(".alert");
  alerts.forEach((alert) => {
    setTimeout(() => {
      alert.style.opacity = "0";
      alert.style.transition = "opacity 0.4s ease";
      setTimeout(() => alert.remove(), 400);
    }, 3000);
  });
});

// Hàm Bật/Tắt trạng thái sản phẩm
function toggleStatus(productId, element) {
  // Lấy mã bảo mật AntiForgeryToken có sẵn trên trang (từ form Delete)
  const token = document.querySelector(
    'input[name="__RequestVerificationToken"]',
  ).value;

  // Gửi request ngầm xuống Controller
  fetch(`/Admin/Product/ToggleStatus/${productId}`, {
    method: "POST",
    headers: {
      RequestVerificationToken: token,
      "Content-Type": "application/json",
    },
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        // Nếu C# báo thành công, tiến hành đổi màu badge trên giao diện
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
      alert("Không thể kết nối đến server.");
    });
}

// Hàm mở Modal chi tiết đánh giá
function openReviewModal(customer, product, rating, comment) {
  // Điền dữ liệu vào form
  document.getElementById("mdlCustomer").innerText = customer || "Unknown";
  document.getElementById("mdlProduct").innerText =
    product || "Deleted Product";
  document.getElementById("mdlComment").innerText = '"' + comment + '"';

  // Vẽ số sao
  let starsHtml = "";
  for (let i = 1; i <= 5; i++) {
    starsHtml +=
      i <= rating
        ? '<span class="rev-star-filled">★</span>'
        : '<span class="rev-star-empty">★</span>';
  }
  document.getElementById("mdlRating").innerHTML = starsHtml;

  // Hiển thị modal
  document.getElementById("reviewDetailModal").classList.add("show");
}

function closeReviewModal() {
  document.getElementById("reviewDetailModal").classList.remove("show");
}

// Bấm ra vùng đen bên ngoài cũng tự đóng Modal
window.onclick = function (event) {
  let modal = document.getElementById("reviewDetailModal");
  if (event.target == modal) {
    closeReviewModal();
  }
};
