document.addEventListener('DOMContentLoaded', function() {
    const modal = document.getElementById('registerModal');
    const registerBtn = document.querySelector('.register');
    const closeBtn = document.querySelector('.close');
    const registerForm = document.getElementById('registerForm');
    const loginForm = document.getElementById('loginForm');

    // Функция для отображения сообщений
    function showMessage(message, type, form) {
        const container = form.querySelector('.error-container');
        const messageDiv = document.createElement('div');
        messageDiv.className = type === 'error' ? 'error-message' : 'success-message';
        messageDiv.textContent = message;
        container.innerHTML = '';
        container.appendChild(messageDiv);
        
        setTimeout(() => messageDiv.remove(), 3000);
    }

    // Обработка формы логина
    loginForm.onsubmit = async function(e) {
        e.preventDefault();

        const username = document.getElementById('loginUsername').value;
        const password = document.getElementById('loginPassword').value;

        try {
            const response = await fetch('https://localhost:7037/api/Auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            const data = await response.text();
            console.log('Server response:', data);

            if (response.ok) {
                try {
                    const jsonData = JSON.parse(data);
                    localStorage.setItem('token', jsonData.token);
                    localStorage.setItem('refreshToken', jsonData.refreshToken);
                    showMessage('Вход выполнен успешно!', 'success', loginForm);
                    loginForm.reset();
                } catch (e) {
                    console.error('JSON parse error:', e);
                    showMessage('Ошибка обработки ответа сервера', 'error', loginForm);
                }
            } else {
                // Удаляем кавычки и пробелы из ответа
                const errorText = data.replace(/['"]/g, '').trim();
                console.log('Error text:', errorText);

                if (errorText === 'Аккаунт заблокирован') {
                    showMessage('Аккаунт заблокирован, обратитесь к администратору', 'error', loginForm);
                } else if (errorText === 'Неверное имя пользователя или пароль') {
                    showMessage('Неправильный логин или пароль', 'error', loginForm);
                } else {
                    showMessage(errorText || 'Ошибка при входе', 'error', loginForm);
                }
            }
        } catch (error) {
            console.error('Network error:', error);
            showMessage('Ошибка при подключении к серверу', 'error', loginForm);
        }
    };

    // Обработка формы регистрации
    registerForm.onsubmit = async function(e) {
        e.preventDefault();

        const username = document.getElementById('regUsername').value;
        const password = document.getElementById('regPassword').value;

        try {
            const response = await fetch('https://localhost:7037/api/Auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            const data = await response.text();
            console.log('Register response:', data);

            if (response.ok) {
                showMessage('Регистрация успешна!', 'success', registerForm);
                setTimeout(() => {
                    modal.style.display = "none";
                    registerForm.reset();
                }, 1500);
            } else {
                const errorText = data.replace(/['"]/g, '').trim();
                showMessage(errorText || 'Ошибка при регистрации', 'error', registerForm);
            }
        } catch (error) {
            console.error('Register error:', error);
            showMessage('Ошибка при подключении к серверу', 'error', registerForm);
        }
    };

    // Открытие модального окна
    registerBtn.onclick = function() {
        modal.style.display = "block";
    }

    // Закрытие модального окна
    closeBtn.onclick = function() {
        modal.style.display = "none";
    }

    // Закрытие при клике вне модального окна
    window.onclick = function(event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
});