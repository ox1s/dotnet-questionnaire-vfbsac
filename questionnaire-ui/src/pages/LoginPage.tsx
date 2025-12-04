import React, { useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button'; 

const LoginPage: React.FC = () => {
    const [login, setLogin] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const authLogin = useAuthStore((state) => state.login);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        try {
            await authLogin({ Login: login, Password: password });
            navigate('/');
        } catch (err) {
            setError('Неверный логин или пароль');
        }
    };

    return (
        <div className="flex items-center justify-center min-h-screen w-full bg-gray-50 dark:bg-gray-900">
            <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-xl shadow-lg dark:bg-gray-800">
                <h1 className="text-3xl font-bold text-center text-gray-900 dark:text-white">
                    Вход в систему
                </h1>
                <form onSubmit={handleSubmit} className="space-y-6">
                    <div>
                        <label 
                            htmlFor="login" 
                            className="block text-sm font-medium text-gray-700 dark:text-gray-300"
                        >
                            Логин
                        </label>
                        <input
                            id="login"
                            type="text"
                            required
                            autoFocus
                            className="mt-1 block w-full px-4 py-2 text-gray-900 bg-white border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                            value={login}
                            onChange={(e) => setLogin(e.target.value)}
                        />
                    </div>
                    <div>
                        <label 
                            htmlFor="password"  
                            className="block text-sm font-medium text-gray-700 dark:text-gray-300"
                        >
                            Пароль
                        </label>
                         <input
                            id="password"
                            type="password"
                            required
                            className="mt-1 block w-full px-4 py-2 text-gray-900 bg-white border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>
                    {error && (
                        <p className="text-sm text-red-500 dark:text-red-400">
                            {error}
                        </p>
                    )}
                    <Button type="submit" className="w-full">
                        Войти
                    </Button>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;