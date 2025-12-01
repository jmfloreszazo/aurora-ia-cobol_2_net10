import React, { useState } from 'react';
import { Link, useLocation, Outlet } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  adminOnly?: boolean;
}

interface MainLayoutProps {
  children?: React.ReactNode;
}

const navItems: NavItem[] = [
  {
    label: 'Dashboard',
    path: '/dashboard',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
      </svg>
    ),
  },
  {
    label: 'Accounts',
    path: '/accounts',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
      </svg>
    ),
  },
  {
    label: 'Cards',
    path: '/cards',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
      </svg>
    ),
  },
  {
    label: 'Transactions',
    path: '/transactions',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
      </svg>
    ),
  },
  {
    label: 'Bill Payment',
    path: '/billing',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
    ),
  },
  {
    label: 'Reports',
    path: '/reports',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    ),
  },
  {
    label: 'Users',
    path: '/admin/users',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
      </svg>
    ),
    adminOnly: true,
  },
];

export const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const isAdmin = user?.userType === 'Admin';
  const filteredNavItems = navItems.filter(item => !item.adminOnly || isAdmin);

  const currentDate = new Date().toLocaleDateString('en-US', {
    month: '2-digit',
    day: '2-digit',
    year: '2-digit',
  });

  const currentTime = new Date().toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  });

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Top Header - COBOL Style */}
      <header className="bg-blue-900 text-white">
        <div className="flex items-center justify-between px-4 py-2">
          <div className="flex items-center space-x-4">
            <span className="text-blue-300 text-sm">Tran: <span className="text-white">CARD</span></span>
            <span className="text-yellow-400 font-bold text-lg">CardDemo - Credit Card Management System</span>
          </div>
          <div className="flex items-center space-x-4 text-sm">
            <span className="text-blue-300">Date: <span className="text-white">{currentDate}</span></span>
            <span className="text-blue-300">Time: <span className="text-white">{currentTime}</span></span>
          </div>
        </div>
        <div className="flex items-center justify-between px-4 py-1 bg-blue-800">
          <span className="text-blue-300 text-sm">Prog: <span className="text-white">REACT-UI</span></span>
          <div className="flex items-center space-x-4">
            <span className="text-sm">
              User: <span className="font-medium text-yellow-400">{user?.fullName}</span>
            </span>
            <span className={`px-2 py-0.5 text-xs font-medium rounded ${isAdmin ? 'bg-red-600' : 'bg-green-600'}`}>
              {user?.userType}
            </span>
            <button
              onClick={logout}
              className="px-3 py-1 text-xs font-medium bg-red-700 hover:bg-red-600 rounded"
            >
              F3=Exit
            </button>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className={`${sidebarOpen ? 'w-64' : 'w-16'} bg-gray-800 min-h-[calc(100vh-76px)] transition-all duration-300`}>
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="w-full p-3 text-gray-400 hover:text-white hover:bg-gray-700 flex items-center justify-center"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {sidebarOpen ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
              )}
            </svg>
          </button>
          
          <nav className="mt-2">
            {filteredNavItems.map((item) => {
              const isActive = location.pathname === item.path || location.pathname.startsWith(item.path + '/');
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`flex items-center px-4 py-3 text-sm transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white border-l-4 border-yellow-400'
                      : 'text-gray-300 hover:bg-gray-700 hover:text-white border-l-4 border-transparent'
                  }`}
                >
                  <span className="flex-shrink-0">{item.icon}</span>
                  {sidebarOpen && <span className="ml-3">{item.label}</span>}
                </Link>
              );
            })}
          </nav>

          {sidebarOpen && (
            <div className="absolute bottom-4 left-4 text-xs text-gray-500">
              <p>CardDemo v2.0</p>
              <p>Modernized from COBOL</p>
            </div>
          )}
        </aside>

        {/* Main Content - Support both children prop and Outlet */}
        <main className="flex-1 p-6">
          {children || <Outlet />}
        </main>
      </div>

      {/* Footer - COBOL Style Function Keys */}
      <footer className="fixed bottom-0 left-0 right-0 bg-yellow-400 text-black px-4 py-1 text-sm font-mono">
        <div className="flex items-center space-x-6">
          <span>ENTER=Continue</span>
          <span>F3=Exit</span>
          <span>F4=Clear</span>
          <span>F7=Backward</span>
          <span>F8=Forward</span>
        </div>
      </footer>
    </div>
  );
};
