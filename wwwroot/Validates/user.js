// File: wwwroot/Validates/user-validation.js

document.addEventListener('DOMContentLoaded', function () {
    // Nhận diện xem đang là Form Thêm (addUserForm) hay Sửa (editUserForm)
    const form = document.getElementById('addUserForm') || document.getElementById('editUserForm');
    
    if (form) {
        const isEditMode = form.id === 'editUserForm'; // Biến cờ xác định Edit Mode

        // Mẹo UX: Xóa viền đỏ khi người dùng gõ
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.addEventListener('input', function() {
                this.style.borderColor = '';
                this.classList.remove('border-red');
                const errorMsg = this.parentNode.querySelector('.error-msg');
                if (errorMsg) {
                    errorMsg.innerText = ''; 
                }
            });
        });

        form.addEventListener('submit', function (e) {
            let isValid = true;

            const usernameInput = form.querySelector('input[name="UserName"]');
            const emailInput = form.querySelector('input[name="Email"]');
            const phoneInput = form.querySelector('input[name="PhoneNumber"]');
            
            // Trang Add thì name="password", trang Edit thì name="newPassword"
            const passwordInput = form.querySelector('input[name="password"]') || form.querySelector('input[name="newPassword"]');

            // Xóa báo lỗi cũ
            form.querySelectorAll('.error-msg').forEach(el => el.innerText = '');
            form.querySelectorAll('.border-red').forEach(el => {
                el.classList.remove('border-red');
                el.style.borderColor = '';
            });

            function showError(inputElement, message) {
                isValid = false;
                inputElement.style.borderColor = '#d93025';
                inputElement.classList.add('border-red');
                
                let errorSpan = inputElement.parentNode.querySelector('.error-msg');
                if (!errorSpan) {
                    errorSpan = document.createElement('span');
                    errorSpan.className = 'text-danger error-msg';
                    errorSpan.style.fontSize = '12px';
                    errorSpan.style.marginTop = '4px';
                    errorSpan.style.display = 'block';
                    inputElement.parentNode.appendChild(errorSpan);
                }
                errorSpan.innerText = message;
            }

            // 1. Kiểm tra Username
            if (usernameInput.value.trim() === '') {
                showError(usernameInput, 'Vui lòng nhập Username.');
            } else if (usernameInput.value.trim().length < 4) {
                showError(usernameInput, 'Username phải có ít nhất 4 ký tự.');
            }

            // 2. Kiểm tra Password (LOGIC THÔNG MINH MỚI)
            if (passwordInput) {
                if (!isEditMode && passwordInput.value.trim() === '') {
                    // Nếu là form Add (Tạo mới) -> Bắt buộc nhập
                    showError(passwordInput, 'Mật khẩu không được để trống.');
                } else if (passwordInput.value.trim() !== '' && passwordInput.value.length < 6) {
                    // Nếu đã nhập (cho cả Add lẫn Edit) -> Phải >= 6 ký tự
                    showError(passwordInput, 'Mật khẩu phải từ 6 ký tự trở lên.');
                }
            }

            // 3. Kiểm tra Email
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (emailInput.value.trim() === '') {
                showError(emailInput, 'Vui lòng nhập địa chỉ Email.');
            } else if (!emailRegex.test(emailInput.value.trim())) {
                showError(emailInput, 'Định dạng Email không hợp lệ.');
            }

            // 4. Kiểm tra Phone
            const phoneRegex = /^[0-9]{10,11}$/;
            if (phoneInput.value.trim() === '') {
                showError(phoneInput, 'Vui lòng nhập số điện thoại.');
            } else if (!phoneRegex.test(phoneInput.value.trim())) {
                showError(phoneInput, 'Số điện thoại không hợp lệ (10-11 số).');
            }

            if (!isValid) {
                e.preventDefault(); 
            }
        });
    }
});