import React, { useState } from 'react';
import { Button } from './components/ui/button';
import { Card, CardHeader, CardTitle, CardContent } from './components/ui/card';
import { Badge } from './components/ui/badge';
import { AdminDashboard } from './components/AdminDashboard';
import { AuthorityDashboard } from './components/AuthorityDashboard';
import { mockUsers } from './lib/mock-data';
import { Toaster } from './components/ui/sonner';

export default function App() {
  const [currentUser, setCurrentUser] = useState(mockUsers[0]); // Start with admin user
  const [currentView, setCurrentView] = useState<'login' | 'dashboard'>('login');

  const handleLogin = (user: typeof mockUsers[0]) => {
    setCurrentUser(user);
    setCurrentView('dashboard');
  };

  const handleLogout = () => {
    setCurrentView('login');
  };

  if (currentView === 'dashboard') {
    return (
      <div className="min-h-screen">
        <Toaster position="top-center" />
        
        {/* Logout Button */}
        <div className="absolute top-4 left-4 z-50">
          <Button variant="outline" onClick={handleLogout}>
            تسجيل الخروج
          </Button>
        </div>

        {currentUser.role === 'admin' ? (
          <AdminDashboard currentUser={currentUser} />
        ) : (
          <AuthorityDashboard currentUser={currentUser} />
        )}
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center rtl" dir="rtl">
      <Toaster position="top-center" />
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">نظام إدارة البلاغات</CardTitle>
          <p className="text-gray-600">اختر المستخدم لتسجيل الدخول</p>
        </CardHeader>
        <CardContent className="space-y-4">
          {mockUsers
            .filter(user => user.role === 'admin' || user.role === 'authority')
            .map((user) => (
            <div key={user.id} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <h3 className="font-medium">{user.name}</h3>
                <Badge variant={user.role === 'admin' ? 'default' : 'secondary'}>
                  {user.role === 'admin' ? 'مدير' : 'جهة حكومية'}
                </Badge>
              </div>
              <p className="text-sm text-gray-600 mb-2">{user.email}</p>
              {user.authority && (
                <p className="text-sm text-gray-500 mb-3">{user.authority}</p>
              )}
              <Button 
                onClick={() => handleLogin(user)} 
                className="w-full"
                variant={user.role === 'admin' ? 'default' : 'outline'}
              >
                تسجيل الدخول
              </Button>
            </div>
          ))}
          
          <div className="text-center pt-4 border-t">
            <p className="text-sm text-gray-500">
              هذا نظام تجريبي يحتوي على بيانات وهمية لتوضيح الوظائف
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}