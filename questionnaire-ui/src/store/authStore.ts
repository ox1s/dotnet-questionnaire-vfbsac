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
        console.log('getRolesFromToken: Decoding token:', token);
        const decoded = jwtDecode<DecodedToken>(token);
        console.log('getRolesFromToken: Decoded token object:', decoded);
        
        // ВАЖНО: Проверяем, как именно называется поле с ролями в вашем токене
        // В ASP.NET Core по умолчанию это: http://schemas.microsoft.com/ws/2008/06/identity/claims/role
        const roleClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        console.log('getRolesFromToken: Extracted role claim:', roleClaim);

        if (Array.isArray(roleClaim)) return roleClaim;
        if (typeof roleClaim === 'string') return [roleClaim];
        
        console.log('getRolesFromToken: Role claim is not a string or array, returning empty.');
        return [];
    } catch (error) {
        console.error('getRolesFromToken: Failed to decode token', error);
        return [];
    }
};

export const useAuthStore = create<AuthState>((set) => ({
    token: null,
    isAuthenticated: false,
    isLoading: true,
    roles: [],

    login: async (credentials) => {
        try {
            const response = await apiClient.post('/auth/login', credentials);
            const { token } = response.data;
            console.log('Login successful, received token:', token);
            localStorage.setItem('authToken', token);
            set({ token, isAuthenticated: true, roles: getRolesFromToken(token) });
        } catch (error) {
            console.error('Login failed in store:', error);
            set({ isLoading: false }); 
            throw error;
        }
    },

    logout: () => {
        localStorage.removeItem('authToken');
        set({ token: null, isAuthenticated: false, roles: [] }); 
    },

    initialize: () => {
        try {
            const token = localStorage.getItem('authToken');
            console.log('Initializing with token from localStorage:', token);
            if (token) {
                set({ token, isAuthenticated: true, roles: getRolesFromToken(token) });
            }
        } finally {
            set({ isLoading: false });
        }
    },
}));