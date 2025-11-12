import axios from 'axios';

// Создаем экземпляр axios с базовой конфигурацией
const apiClient = axios.create({
    baseURL: 'http://localhost:5202', // URL нашего backend'а
    headers: {
        'Content-Type': 'application/json',
    },
});

// Создаем перехватчик (interceptor) для всех исходящих запросов
apiClient.interceptors.request.use(
    (config) => {
        // Пытаемся получить токен из localStorage
        const token = localStorage.getItem('authToken');
        
        // Если токен есть, добавляем его в заголовок Authorization
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        
        return config;
    },
    (error) => {
        // В случае ошибки просто пробрасываем ее дальше
        return Promise.reject(error);
    }
);

export default apiClient;