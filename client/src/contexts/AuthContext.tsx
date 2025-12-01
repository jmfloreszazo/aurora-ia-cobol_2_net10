import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { authService } from '@/services/authService';
import type { User, LoginRequest } from '@/types';

interface AuthContextType {
  user: User | null;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = authService.getToken();
    const currentUser = authService.getCurrentUser();
    
    if (token && currentUser) {
      setUser(currentUser);
    }
    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    const userData: User = {
      userId: response.userId,
      fullName: response.fullName,
      userType: response.userType,
      token: response.token
    };
    
    authService.setAuth(response.token, userData);
    setUser(userData);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        isAuthenticated: !!user,
        isLoading
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
