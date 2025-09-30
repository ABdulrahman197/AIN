import React, { useState, useEffect } from 'react';
import { Users, FileText, BarChart, Search, Filter, Edit, Trash2, Eye, UserPlus } from 'lucide-react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Card, CardHeader, CardTitle, CardContent } from './ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
import { Badge } from './ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from './ui/dialog';
import { Label } from './ui/label';
import { Textarea } from './ui/textarea';
import { toast } from 'sonner@2.0.3';
import { adminApi, reportsApi } from '../lib/api';
import { User, Report } from '../lib/mock-data';

interface AdminDashboardProps {
  currentUser: User;
}

export function AdminDashboard({ currentUser }: AdminDashboardProps) {
  const [users, setUsers] = useState<User[]>([]);
  const [reports, setReports] = useState<Report[]>([]);
  const [filteredReports, setFilteredReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'reports' | 'users' | 'stats'>('reports');
  
  // Filters
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  const [priorityFilter, setPriorityFilter] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    filterReports();
  }, [reports, statusFilter, categoryFilter, priorityFilter, searchTerm]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [usersData, reportsData] = await Promise.all([
        adminApi.getAllUsers(),
        adminApi.getAllReports()
      ]);
      setUsers(usersData);
      setReports(reportsData);
    } catch (error) {
      toast.error('حدث خطأ في تحميل البيانات');
    } finally {
      setLoading(false);
    }
  };

  const filterReports = () => {
    let filtered = reports;

    if (statusFilter) {
      filtered = filtered.filter(report => report.status === statusFilter);
    }
    if (categoryFilter) {
      filtered = filtered.filter(report => report.category === categoryFilter);
    }
    if (priorityFilter) {
      filtered = filtered.filter(report => report.priority === priorityFilter);
    }
    if (searchTerm) {
      filtered = filtered.filter(report => 
        report.title.includes(searchTerm) || 
        report.description.includes(searchTerm) ||
        report.citizenName.includes(searchTerm)
      );
    }

    setFilteredReports(filtered);
    setCurrentPage(1);
  };

  const handleDeleteReport = async (reportId: string) => {
    try {
      await adminApi.deleteReport(reportId);
      setReports(reports.filter(report => report.id !== reportId));
      toast.success('تم حذف البلاغ بنجاح');
    } catch (error) {
      toast.error('حدث خطأ في حذف البلاغ');
    }
  };

  const handleDeleteUser = async (userId: string) => {
    try {
      await adminApi.deleteUser(userId);
      setUsers(users.filter(user => user.id !== userId));
      toast.success('تم حذف المستخدم بنجاح');
    } catch (error) {
      toast.error('حدث خطأ في حذف المستخدم');
    }
  };

  const handleUpdateReportStatus = async (reportId: string, newStatus: string) => {
    try {
      const updatedReport = await reportsApi.updateReportStatus(reportId, newStatus);
      setReports(reports.map(report => 
        report.id === reportId ? updatedReport : report
      ));
      toast.success('تم تحديث حالة البلاغ بنجاح');
    } catch (error) {
      toast.error('حدث خطأ في تحديث حالة البلاغ');
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'in_progress': return 'bg-blue-100 text-blue-800';
      case 'resolved': return 'bg-green-100 text-green-800';
      case 'rejected': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'pending': return 'في الانتظار';
      case 'in_progress': return 'قيد المعالجة';
      case 'resolved': return 'تم الحل';
      case 'rejected': return 'مرفوض';
      default: return status;
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return 'bg-red-100 text-red-800';
      case 'medium': return 'bg-yellow-100 text-yellow-800';
      case 'low': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getPriorityText = (priority: string) => {
    switch (priority) {
      case 'high': return 'عالية';
      case 'medium': return 'متوسطة';
      case 'low': return 'منخفضة';
      default: return priority;
    }
  };

  const getVisibilityBadge = (visibility: 1 | 2 | 3) => {
    const text = visibility === 1 ? 'عامة' : visibility === 2 ? 'سري' : 'مجهول';
    const cls = visibility === 1
      ? 'bg-green-100 text-green-800'
      : visibility === 2
      ? 'bg-amber-100 text-amber-800'
      : 'bg-gray-200 text-gray-800';
    return <Badge className={cls}>{text}</Badge>;
  };

  // Pagination
  const totalPages = Math.ceil(filteredReports.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const currentReports = filteredReports.slice(startIndex, endIndex);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary mx-auto mb-4"></div>
          <p>جاري تحميل البيانات...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 rtl" dir="rtl">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-semibold text-gray-900">لوحة تحكم المدير</h1>
              <p className="text-gray-600">مرحباً {currentUser.name}</p>
            </div>
            <div className="flex items-center gap-3">
              <Button variant="outline" size="sm">
                <UserPlus className="w-4 h-4 ml-2" />
                إضافة مستخدم جديد
              </Button>
            </div>
          </div>
        </div>
      </div>

      <div className="flex">
        {/* Sidebar */}
        <div className="w-64 bg-white shadow-sm min-h-screen">
          <nav className="p-4">
            <div className="space-y-2">
              <button
                onClick={() => setActiveTab('reports')}
                className={`w-full flex items-center px-4 py-2 text-right rounded-lg transition-colors ${
                  activeTab === 'reports' ? 'bg-primary text-white' : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <FileText className="w-5 h-5 ml-3" />
                البلاغات
              </button>
              <button
                onClick={() => setActiveTab('users')}
                className={`w-full flex items-center px-4 py-2 text-right rounded-lg transition-colors ${
                  activeTab === 'users' ? 'bg-primary text-white' : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <Users className="w-5 h-5 ml-3" />
                المستخدمون
              </button>
              <button
                onClick={() => setActiveTab('stats')}
                className={`w-full flex items-center px-4 py-2 text-right rounded-lg transition-colors ${
                  activeTab === 'stats' ? 'bg-primary text-white' : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <BarChart className="w-5 h-5 ml-3" />
                الإحصائيات
              </button>
            </div>
          </nav>
        </div>

        {/* Main Content */}
        <div className="flex-1 p-6">
          {/* Statistics Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">إجمالي البلاغات</CardTitle>
                <FileText className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{reports.length}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">البلاغات المعلقة</CardTitle>
                <FileText className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-yellow-600">
                  {reports.filter(r => r.status === 'pending').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">البلاغات المحلولة</CardTitle>
                <FileText className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-green-600">
                  {reports.filter(r => r.status === 'resolved').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">إجمالي المستخدمين</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{users.length}</div>
              </CardContent>
            </Card>
          </div>

          {/* Reports Tab */}
          {activeTab === 'reports' && (
            <Card>
              <CardHeader>
                <CardTitle>إدارة البلاغات</CardTitle>
                
                {/* Filters */}
                <div className="flex flex-wrap gap-4 mt-4">
                  <div className="flex-1 min-w-64">
                    <Input
                      placeholder="البحث في البلاغات..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="w-full"
                    />
                  </div>
                  <Select value={statusFilter || "all"} onValueChange={(value) => setStatusFilter(value === "all" ? "" : value)}>
                    <SelectTrigger className="w-40">
                      <SelectValue placeholder="الحالة" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">جميع الحالات</SelectItem>
                      <SelectItem value="pending">في الانتظار</SelectItem>
                      <SelectItem value="in_progress">قيد المعالجة</SelectItem>
                      <SelectItem value="resolved">تم الحل</SelectItem>
                      <SelectItem value="rejected">مرفوض</SelectItem>
                    </SelectContent>
                  </Select>
                  <Select value={priorityFilter || "all"} onValueChange={(value) => setPriorityFilter(value === "all" ? "" : value)}>
                    <SelectTrigger className="w-40">
                      <SelectValue placeholder="الأولوية" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">جميع الأولويات</SelectItem>
                      <SelectItem value="high">عالية</SelectItem>
                      <SelectItem value="medium">متوسطة</SelectItem>
                      <SelectItem value="low">منخفضة</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardHeader>
              <CardContent>
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="text-right">العنوان</TableHead>
                        <TableHead className="text-right">المواطن</TableHead>
                        <TableHead className="text-right">الظهور</TableHead>
                        <TableHead className="text-right">الفئة</TableHead>
                        <TableHead className="text-right">الأولوية</TableHead>
                        <TableHead className="text-right">الحالة</TableHead>
                        <TableHead className="text-right">الجهة المختصة</TableHead>
                        <TableHead className="text-right">تاريخ الإنشاء</TableHead>
                        <TableHead className="text-right">الإجراءات</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {currentReports.map((report) => (
                        <TableRow key={report.id}>
                          <TableCell className="font-medium">{report.title}</TableCell>
                          <TableCell>{report.citizenName}</TableCell>
                          <TableCell>{getVisibilityBadge(report.visibility)}</TableCell>
                          <TableCell>{report.category}</TableCell>
                          <TableCell>
                            <Badge className={getPriorityColor(report.priority)}>
                              {getPriorityText(report.priority)}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <Badge className={getStatusColor(report.status)}>
                              {getStatusText(report.status)}
                            </Badge>
                          </TableCell>
                          <TableCell>{report.authorityName}</TableCell>
                          <TableCell>{report.createdAt}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Dialog>
                                <DialogTrigger>
                                  <Button variant="outline" size="sm">
                                    <Eye className="w-4 h-4" />
                                  </Button>
                                </DialogTrigger>
                                <DialogContent className="max-w-2xl" dir="rtl">
                                  <DialogHeader>
                                    <DialogTitle>تفاصيل البلاغ</DialogTitle>
                                  </DialogHeader>
                                  <div className="space-y-4">
                                    <div>
                                      <Label>العنوان</Label>
                                      <p className="mt-1">{report.title}</p>
                                    </div>
                                    <div>
                                      <Label>الوصف</Label>
                                      <p className="mt-1">{report.description}</p>
                                    </div>
                                    <div className="grid grid-cols-3 gap-4">
                                      <div>
                                        <Label>المواطن</Label>
                                        <p className="mt-1">{report.citizenName}</p>
                                      </div>
                                      <div>
                                        <Label>الظهور</Label>
                                        <div className="mt-1">{getVisibilityBadge(report.visibility)}</div>
                                      </div>
                                      <div>
                                        <Label>الموقع</Label>
                                        <p className="mt-1">{report.location}</p>
                                      </div>
                                    </div>
                                    <div className="grid grid-cols-3 gap-4">
                                      <div>
                                        <Label>الفئة</Label>
                                        <p className="mt-1">{report.category}</p>
                                      </div>
                                      <div>
                                        <Label>الأولوية</Label>
                                        <Badge className={getPriorityColor(report.priority)}>
                                          {getPriorityText(report.priority)}
                                        </Badge>
                                      </div>
                                      <div>
                                        <Label>الحالة</Label>
                                        <Badge className={getStatusColor(report.status)}>
                                          {getStatusText(report.status)}
                                        </Badge>
                                      </div>
                                    </div>
                                    <div>
                                      <Label>تغيير الحالة</Label>
                                      <Select onValueChange={(value) => value !== "placeholder" && handleUpdateReportStatus(report.id, value)}>
                                        <SelectTrigger className="w-full mt-1">
                                          <SelectValue placeholder="اختر الحالة الجديدة" />
                                        </SelectTrigger>
                                        <SelectContent>
                                          <SelectItem value="placeholder" disabled>اختر الحالة الجديدة</SelectItem>
                                          <SelectItem value="pending">في الانتظار</SelectItem>
                                          <SelectItem value="in_progress">قيد المعالجة</SelectItem>
                                          <SelectItem value="resolved">تم الحل</SelectItem>
                                          <SelectItem value="rejected">مرفوض</SelectItem>
                                        </SelectContent>
                                      </Select>
                                    </div>
                                  </div>
                                </DialogContent>
                              </Dialog>
                              <Button 
                                variant="outline" 
                                size="sm"
                                onClick={() => handleDeleteReport(report.id)}
                              >
                                <Trash2 className="w-4 h-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="flex items-center justify-between mt-4">
                    <p className="text-sm text-gray-700">
                      عرض {startIndex + 1} إلى {Math.min(endIndex, filteredReports.length)} من {filteredReports.length} نتيجة
                    </p>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setCurrentPage(currentPage - 1)}
                        disabled={currentPage === 1}
                      >
                        السابق
                      </Button>
                      <span className="text-sm">
                        صفحة {currentPage} من {totalPages}
                      </span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setCurrentPage(currentPage + 1)}
                        disabled={currentPage === totalPages}
                      >
                        التالي
                      </Button>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* Users Tab */}
          {activeTab === 'users' && (
            <Card>
              <CardHeader>
                <CardTitle>إدارة المستخدمين</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="text-right">الاسم</TableHead>
                        <TableHead className="text-right">البريد الإلكتروني</TableHead>
                        <TableHead className="text-right">الدور</TableHead>
                        <TableHead className="text-right">الجهة</TableHead>
                        <TableHead className="text-right">الحالة</TableHead>
                        <TableHead className="text-right">تاريخ الإنشاء</TableHead>
                        <TableHead className="text-right">الإجراءات</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {users.map((user) => (
                        <TableRow key={user.id}>
                          <TableCell className="font-medium">{user.name}</TableCell>
                          <TableCell>{user.email}</TableCell>
                          <TableCell>
                            <Badge variant={user.role === 'admin' ? 'default' : 'secondary'}>
                              {user.role === 'admin' ? 'مدير' : user.role === 'authority' ? 'جهة حكومية' : 'مواطن'}
                            </Badge>
                          </TableCell>
                          <TableCell>{user.authority || '-'}</TableCell>
                          <TableCell>
                            <Badge variant={user.status === 'active' ? 'default' : 'secondary'}>
                              {user.status === 'active' ? 'نشط' : 'غير نشط'}
                            </Badge>
                          </TableCell>
                          <TableCell>{user.createdAt}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Button variant="outline" size="sm">
                                <Edit className="w-4 h-4" />
                              </Button>
                              <Button 
                                variant="outline" 
                                size="sm"
                                onClick={() => handleDeleteUser(user.id)}
                                disabled={user.id === currentUser.id}
                              >
                                <Trash2 className="w-4 h-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Statistics Tab */}
          {activeTab === 'stats' && (
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>إحصائيات النظام</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <h3 className="text-lg font-medium mb-4">توزيع البلاغات حسب الحالة</h3>
                      <div className="space-y-3">
                        <div className="flex justify-between items-center">
                          <span>في الانتظار</span>
                          <span className="font-medium">{reports.filter(r => r.status === 'pending').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>قيد المعالجة</span>
                          <span className="font-medium">{reports.filter(r => r.status === 'in_progress').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>تم الحل</span>
                          <span className="font-medium">{reports.filter(r => r.status === 'resolved').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>مرفوض</span>
                          <span className="font-medium">{reports.filter(r => r.status === 'rejected').length}</span>
                        </div>
                      </div>
                    </div>
                    <div>
                      <h3 className="text-lg font-medium mb-4">توزيع المستخدمين حسب الدور</h3>
                      <div className="space-y-3">
                        <div className="flex justify-between items-center">
                          <span>المديرون</span>
                          <span className="font-medium">{users.filter(u => u.role === 'admin').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>الجهات الحكومية</span>
                          <span className="font-medium">{users.filter(u => u.role === 'authority').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>المواطنون</span>
                          <span className="font-medium">{users.filter(u => u.role === 'citizen').length}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}