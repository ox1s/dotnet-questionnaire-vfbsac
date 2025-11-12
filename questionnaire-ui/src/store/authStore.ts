import { create } from 'zustand';
import apiClient from '../api/axios';
import type { LoginRequest } from '../types/auth';
import { jwtDecode } from 'jwt-decode'; 

interface DecodedToken {
    role: string | string[]; 
    [key: string]: any;
}

interface AuthState {
    token: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    roles: string[]; 
    login: (credentials: LoginRequest) => Promise<void>;
    logout: () => void;
    initialize: () => void;
}

const getRolesFromToken = (token: string | null): string[] => {
    if (!token) return [];
    try {
        const decoded = jwtDecode<DecodedToken>(token);
        const roles = decoded.role;
        if (Array.isArray(roles)) return roles;
        if (typeof roles === 'string') return [roles];
        return [];
    } catch (error) {
        return [];
    }
};

export const useAuthStore = create<AuthState>((set) => ({
    token: null,
    isAuthenticated: false,
    isLoading: true,
    roles: [], 
    login: async (credentials) => {
        const response = await apiClient.post('/auth/login', credentials);
        const { token } = response.data;
        localStorage.setItem('authToken', token);
        set({ token, isAuthenticated: true, roles: getRolesFromToken(token) }); // <-- ОБНОВЛЯЕМ РОЛИ
    },

    logout: () => {
        localStorage.removeItem('authToken');
        set({ token: null, isAuthenticated: false, roles: [] }); // <-- СБРАСЫВАЕМ РОЛИ
    },

    initialize: () => {
        try {
            const token = localStorage.getItem('authToken');
            if (token) {
                set({ token, isAuthenticated: true, roles: getRolesFromToken(token) }); // <-- ОБНОВЛЯЕМ РОЛИ
            }
        } finally {
            set({ isLoading: false });
        }
    },
}));