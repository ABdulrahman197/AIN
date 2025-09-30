import React, { useState, useEffect } from 'react';
import { FileText, MessageSquare, BarChart, Heart, Clock, CheckCircle, XCircle, Eye } from 'lucide-react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Card, CardHeader, CardTitle, CardContent } from './ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
import { Badge } from './ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from './ui/dialog';
import { Label } from './ui/label';
import { Textarea } from './ui/textarea';
import { ScrollArea } from './ui/scroll-area';
import { Separator } from './ui/separator';
import { toast } from 'sonner@2.0.3';
import { authorityApi, reportsApi } from '../lib/api';
import { User, Report, Comment, Interaction } from '../lib/mock-data';

interface AuthorityDashboardProps {
  currentUser: User;
}

export function AuthorityDashboard({ currentUser }: AuthorityDashboardProps) {
  const [reports, setReports] = useState<Report[]>([]);
  const [filteredReports, setFilteredReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'reports' | 'comments' | 'stats'>('reports');
  
  // Filters
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [priorityFilter, setPriorityFilter] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Comment dialog
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [interactions, setInteractions] = useState<Interaction[]>([]);
  const [newComment, setNewComment] = useState<string>('');
  const [commentDialogOpen, setCommentDialogOpen] = useState(false);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  useEffect(() => {
    loadReports();
  }, []);

  useEffect(() => {
    filterReports();
  }, [reports, statusFilter, priorityFilter, searchTerm]);

  const loadReports = async () => {
    try {
      setLoading(true);
      const reportsData = await authorityApi.getAuthorityReports(currentUser.id);
      setReports(reportsData);
    } catch (error) {
      toast.error('حدث خطأ في تحميل البلاغات');
    } finally {
      setLoading(false);
    }
  };

  const filterReports = () => {
    let filtered = reports;

    if (statusFilter) {
      filtered = filtered.filter(report => report.status === statusFilter);
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

  const handleLikeReport = async (reportId: string) => {
    try {
      await reportsApi.likeReport(reportId, currentUser.id);
      // Update the report likes count
      setReports(reports.map(report => 
        report.id === reportId ? { ...report, likes: report.likes + 1 } : report
      ));
      toast.success('تم إعجابك بالبلاغ');
    } catch (error) {
      toast.error('حدث خطأ في إضافة الإعجاب');
    }
  };

  const handleAddComment = async () => {
    if (!selectedReport || !newComment.trim()) return;

    try {
      const comment = await reportsApi.addComment(selectedReport.id, currentUser.id, newComment);
      setComments([...comments, comment]);
      setNewComment('');
      toast.success('تم إضافة التعليق بنجاح');
    } catch (error) {
      toast.error('حدث خطأ في إضافة التعليق');
    }
  };

  const loadReportDetails = async (report: Report) => {
    try {
      setSelectedReport(report);
      const [commentsData, interactionsData] = await Promise.all([
        reportsApi.getComments(report.id),
        reportsApi.getInteractions(report.id)
      ]);
      setComments(commentsData);
      setInteractions(interactionsData);
      setCommentDialogOpen(true);
    } catch (error) {
      toast.error('حدث خطأ في تحميل تفاصيل البلاغ');
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

  const getAuthorityDisplayName = (report: Report) => {
    // Public (1) & Confidential (2): show real name, Anonymous (3): mask
    return report.visibility === 3 ? 'مجهول' : report.citizenName;
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
          <p>جاري تحميل البلاغات...</p>
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
              <h1 className="text-2xl font-semibold text-gray-900">لوحة تحكم الجهة المختصة</h1>
              <p className="text-gray-600">مرحباً {currentUser.name} - {currentUser.authority}</p>
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
                بلاغاتي
              </button>
              <button
                onClick={() => setActiveTab('comments')}
                className={`w-full flex items-center px-4 py-2 text-right rounded-lg transition-colors ${
                  activeTab === 'comments' ? 'bg-primary text-white' : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <MessageSquare className="w-5 h-5 ml-3" />
                التعليقات
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
                <CardTitle className="text-sm font-medium">في الانتظار</CardTitle>
                <Clock className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-yellow-600">
                  {reports.filter(r => r.status === 'pending').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">تم الحل</CardTitle>
                <CheckCircle className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-green-600">
                  {reports.filter(r => r.status === 'resolved').length}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">الإعجابات</CardTitle>
                <Heart className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-red-600">
                  {reports.reduce((total, report) => total + report.likes, 0)}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Reports Tab */}
          {activeTab === 'reports' && (
            <Card>
              <CardHeader>
                <CardTitle>بلاغاتي المُحالة</CardTitle>
                
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
                        <TableHead className="text-right">الإعجابات</TableHead>
                        <TableHead className="text-right">تاريخ الإنشاء</TableHead>
                        <TableHead className="text-right">الإجراءات</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {currentReports.map((report) => (
                        <TableRow key={report.id}>
                          <TableCell className="font-medium">{report.title}</TableCell>
                          <TableCell>{getAuthorityDisplayName(report)}</TableCell>
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
                          <TableCell>
                            <div className="flex items-center gap-1">
                              <Heart className="w-4 h-4 text-red-500" />
                              <span>{report.likes}</span>
                            </div>
                          </TableCell>
                          <TableCell>{report.createdAt}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => loadReportDetails(report)}
                              >
                                <Eye className="w-4 h-4" />
                              </Button>
                              <Select onValueChange={(value) => value !== "placeholder" && handleUpdateReportStatus(report.id, value)}>
                                <SelectTrigger className="w-32">
                                  <SelectValue placeholder="تغيير الحالة" />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="placeholder" disabled>تغيير الحالة</SelectItem>
                                  <SelectItem value="pending">في الانتظار</SelectItem>
                                  <SelectItem value="in_progress">قيد المعالجة</SelectItem>
                                  <SelectItem value="resolved">تم الحل</SelectItem>
                                  <SelectItem value="rejected">مرفوض</SelectItem>
                                </SelectContent>
                              </Select>
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleLikeReport(report.id)}
                              >
                                <Heart className="w-4 h-4" />
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

          {/* Statistics Tab */}
          {activeTab === 'stats' && (
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>إحصائيات بلاغاتي</CardTitle>
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
                      <h3 className="text-lg font-medium mb-4">توزيع البلاغات حسب الأولوية</h3>
                      <div className="space-y-3">
                        <div className="flex justify-between items-center">
                          <span>عالية</span>
                          <span className="font-medium">{reports.filter(r => r.priority === 'high').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>متوسطة</span>
                          <span className="font-medium">{reports.filter(r => r.priority === 'medium').length}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span>منخفضة</span>
                          <span className="font-medium">{reports.filter(r => r.priority === 'low').length}</span>
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

      {/* Report Details Dialog */}
      <Dialog open={commentDialogOpen} onOpenChange={setCommentDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[80vh]" dir="rtl">
          <DialogHeader>
            <DialogTitle>تفاصيل البلاغ</DialogTitle>
          </DialogHeader>
          {selectedReport && (
            <div className="space-y-6">
              {/* Report Details */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>العنوان</Label>
                  <p className="mt-1 font-medium">{selectedReport.title}</p>
                </div>
                <div>
                  <Label>المواطن</Label>
                  <p className="mt-1">{getAuthorityDisplayName(selectedReport)}</p>
                </div>
                <div>
                  <Label>الظهور</Label>
                  <div className="mt-1">{getVisibilityBadge(selectedReport.visibility)}</div>
                </div>
                <div>
                  <Label>الفئة</Label>
                  <p className="mt-1">{selectedReport.category}</p>
                </div>
                <div>
                  <Label>الموقع</Label>
                  <p className="mt-1">{selectedReport.location}</p>
                </div>
                <div>
                  <Label>الأولوية</Label>
                  <Badge className={getPriorityColor(selectedReport.priority)}>
                    {getPriorityText(selectedReport.priority)}
                  </Badge>
                </div>
                <div>
                  <Label>الحالة</Label>
                  <Badge className={getStatusColor(selectedReport.status)}>
                    {getStatusText(selectedReport.status)}
                  </Badge>
                </div>
              </div>
              
              <div>
                <Label>الوصف</Label>
                <p className="mt-1 p-3 bg-gray-50 rounded-md">{selectedReport.description}</p>
              </div>

              <Separator />

              {/* Comments Section */}
              <div>
                <h3 className="text-lg font-medium mb-4">التعليقات والتفاعلات</h3>
                
                <ScrollArea className="h-64 w-full border rounded-md p-4">
                  <div className="space-y-4">
                    {comments.length === 0 && interactions.length === 0 ? (
                      <p className="text-gray-500 text-center py-8">لا توجد تعليقات أو تفاعلات بعد</p>
                    ) : (
                      <>
                        {/* Comments */}
                        {comments.map((comment) => (
                          <div key={comment.id} className="border-b pb-3">
                            <div className="flex items-center gap-2 mb-1">
                              <MessageSquare className="w-4 h-4 text-blue-500" />
                              <span className="font-medium">{comment.userName}</span>
                              <span className="text-sm text-gray-500">
                                {new Date(comment.createdAt).toLocaleString('ar-SA')}
                              </span>
                            </div>
                            <p className="text-gray-700 mr-6">{comment.content}</p>
                          </div>
                        ))}
                        
                        {/* Interactions */}
                        {interactions.map((interaction) => (
                          <div key={interaction.id} className="border-b pb-3">
                            <div className="flex items-center gap-2">
                              {interaction.type === 'like' && <Heart className="w-4 h-4 text-red-500" />}
                              {interaction.type === 'view' && <Eye className="w-4 h-4 text-gray-500" />}
                              {interaction.type === 'comment' && <MessageSquare className="w-4 h-4 text-blue-500" />}
                              <span className="font-medium">{interaction.userName}</span>
                              <span className="text-sm text-gray-500">
                                {interaction.type === 'like' ? 'أعجب بالبلاغ' : 
                                 interaction.type === 'view' ? 'شاهد البلاغ' : 'علق على البلاغ'}
                              </span>
                              <span className="text-sm text-gray-500">
                                {new Date(interaction.createdAt).toLocaleString('ar-SA')}
                              </span>
                            </div>
                          </div>
                        ))}
                      </>
                    )}
                  </div>
                </ScrollArea>

                {/* Add Comment */}
                <div className="mt-4 space-y-3">
                  <Label>إضافة تعليق</Label>
                  <Textarea
                    placeholder="اكتب تعليقك هنا..."
                    value={newComment}
                    onChange={(e) => setNewComment(e.target.value)}
                    rows={3}
                  />
                  <Button 
                    onClick={handleAddComment}
                    disabled={!newComment.trim()}
                    className="w-full"
                  >
                    إضافة تعليق
                  </Button>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}