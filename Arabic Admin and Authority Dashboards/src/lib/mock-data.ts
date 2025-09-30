// Mock data for the Arabic dashboards

export interface User {
  id: string;
  name: string;
  email: string;
  role: 'admin' | 'authority' | 'citizen';
  authority?: string;
  createdAt: string;
  status: 'active' | 'inactive';
}

export interface Report {
  id: string;
  title: string;
  description: string;
  category: string;
  priority: 'high' | 'medium' | 'low';
  status: 'pending' | 'in_progress' | 'resolved' | 'rejected';
  citizenId: string;
  citizenName: string;
  authorityId: string;
  authorityName: string;
  assignedOfficerId?: string;
  createdAt: string;
  updatedAt: string;
  location: string;
  likes: number;
  // 1 = Public, 2 = Confidential, 3 = Anonymous (matches backend enum)
  visibility: 1 | 2 | 3;
}

export interface Comment {
  id: string;
  reportId: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
}

export interface Interaction {
  id: string;
  reportId: string;
  userId: string;
  userName: string;
  type: 'like' | 'view' | 'comment';
  createdAt: string;
}

// Mock users
export const mockUsers: User[] = [
  {
    id: '1',
    name: 'أحمد محمد',
    email: 'ahmed@example.com',
    role: 'admin',
    createdAt: '2024-01-15',
    status: 'active'
  },
  {
    id: '2',
    name: 'فاطمة علي',
    email: 'fatima@authority.com',
    role: 'authority',
    authority: 'بلدية الرياض',
    createdAt: '2024-02-10',
    status: 'active'
  },
  {
    id: '3',
    name: 'محمد حسن',
    email: 'mohammed@citizen.com',
    role: 'citizen',
    createdAt: '2024-03-05',
    status: 'active'
  },
  {
    id: '4',
    name: 'نور الدين',
    email: 'nour@authority.com',
    role: 'authority',
    authority: 'وزارة النقل',
    createdAt: '2024-01-20',
    status: 'inactive'
  },
  {
    id: '5',
    name: 'سارة أحمد',
    email: 'sara@citizen.com',
    role: 'citizen',
    createdAt: '2024-03-15',
    status: 'active'
  }
];

// Mock reports
export const mockReports: Report[] = [
  {
    id: '1',
    title: 'مشكلة في الإضاءة العامة',
    description: 'إضاءة الشارع لا تعمل في حي الملز',
    category: 'البنية التحتية',
    priority: 'high',
    status: 'pending',
    citizenId: '3',
    citizenName: 'محمد حسن',
    authorityId: '2',
    authorityName: 'بلدية الرياض',
    createdAt: '2024-03-20',
    updatedAt: '2024-03-20',
    location: 'الرياض - حي الملز',
    likes: 5,
    visibility: 1
  },
  {
    id: '2',
    title: 'حفر في الطريق',
    description: 'يوجد حفر كبيرة في الطريق الرئيسي',
    category: 'الطرق والمواصلات',
    priority: 'medium',
    status: 'in_progress',
    citizenId: '5',
    citizenName: 'سارة أحمد',
    authorityId: '4',
    authorityName: 'وزارة النقل',
    assignedOfficerId: '4',
    createdAt: '2024-03-18',
    updatedAt: '2024-03-21',
    location: 'جدة - طريق الملك عبدالعزيز',
    likes: 12,
    visibility: 2
  },
  {
    id: '3',
    title: 'تسريب مياه',
    description: 'تسريب مياه في الشارع يسبب إزعاج للمواطنين',
    category: 'المرافق العامة',
    priority: 'high',
    status: 'resolved',
    citizenId: '3',
    citizenName: 'محمد حسن',
    authorityId: '2',
    authorityName: 'بلدية الرياض',
    assignedOfficerId: '2',
    createdAt: '2024-03-15',
    updatedAt: '2024-03-22',
    location: 'الرياض - حي العليا',
    likes: 8,
    visibility: 3
  },
  {
    id: '4',
    title: 'مشكلة في جمع النفايات',
    description: 'عدم انتظام جمع النفايات في الحي',
    category: 'النظافة العامة',
    priority: 'low',
    status: 'rejected',
    citizenId: '5',
    citizenName: 'سارة أحمد',
    authorityId: '2',
    authorityName: 'بلدية الرياض',
    createdAt: '2024-03-10',
    updatedAt: '2024-03-19',
    location: 'الرياض - حي الروضة',
    likes: 3,
    visibility: 1
  }
];

// Mock comments
export const mockComments: Comment[] = [
  {
    id: '1',
    reportId: '1',
    userId: '2',
    userName: 'فاطمة علي',
    content: 'تم استلام البلاغ وسيتم حل المشكلة خلال 48 ساعة',
    createdAt: '2024-03-21T10:30:00Z'
  },
  {
    id: '2',
    reportId: '2',
    userId: '4',
    userName: 'نور الدين',
    content: 'جاري العمل على إصلاح الحفر',
    createdAt: '2024-03-21T14:15:00Z'
  },
  {
    id: '3',
    reportId: '3',
    userId: '2',
    userName: 'فاطمة علي',
    content: 'تم حل المشكلة بنجاح',
    createdAt: '2024-03-22T09:45:00Z'
  }
];

// Mock interactions
export const mockInteractions: Interaction[] = [
  {
    id: '1',
    reportId: '1',
    userId: '3',
    userName: 'محمد حسن',
    type: 'like',
    createdAt: '2024-03-21T11:00:00Z'
  },
  {
    id: '2',
    reportId: '2',
    userId: '5',
    userName: 'سارة أحمد',
    type: 'view',
    createdAt: '2024-03-21T15:30:00Z'
  },
  {
    id: '3',
    reportId: '1',
    userId: '2',
    userName: 'فاطمة علي',
    type: 'comment',
    createdAt: '2024-03-21T10:30:00Z'
  }
];