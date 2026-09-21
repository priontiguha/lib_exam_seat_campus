import { lazy, Suspense, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import ToastStack from './components/feedback/ToastStack';
import AuthView from './components/auth/AuthView';
const OverviewSection = lazy(() => import('./components/dashboard/OverviewSection'));
const NotificationsSection = lazy(() => import('./components/features/NotificationsSection'));
const UsersSection = lazy(() => import('./components/features/UsersSection'));
const StudentActivity = lazy(() => import('./components/features/StudentActivity'));
const LibrarySection = lazy(() => import('./components/features/LibrarySection'));
const ExamSection = lazy(() => import('./components/features/ExamSection'));
import Sidebar from './components/layout/Sidebar';
import Topbar from './components/layout/Topbar';
import { useTheme } from './hooks/useTheme';
import { useToast } from './hooks/useToast';
import { useUserActions } from './hooks/useUserActions';
import { useNotificationActions } from './hooks/useNotificationActions';
import { useAuthActions } from './hooks/useAuthActions';
import './App.css';

const initialLoginForm = {
  email: 'admin@library.edu',
  password: 'Admin@123',
};

const initialUserForm = {
  name: '',
  rollNo: '',
  dept: '',
  semester: '1',
  email: '',
  password: '',
  role: 'Student',
  isVerified: false,
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5121';

function App() {
  const [loginForm, setLoginForm] = useState(initialLoginForm);
  const [userForm, setUserForm] = useState(initialUserForm);
  const [status, setStatus] = useState({ type: '', message: '' });
  const [loadingResources, setLoadingResources] = useState({
    dashboard: false,
    library: false,
    exams: false,
    users: false,
    student: false,
  });
  const [dashboardAnalytics, setDashboardAnalytics] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { theme, toggleTheme } = useTheme();
  const { toasts, dismissToast } = useToast(status);
  const [activeSection, setActiveSection] = useState('overview');
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [topSearch, setTopSearch] = useState('');
  const [currentUser, setCurrentUser] = useState(() => {
    const saved = localStorage.getItem('libraryUser');
    return saved ? JSON.parse(saved) : null;
  });
  const [users, setUsers] = useState([]);
  const [editingUserId, setEditingUserId] = useState(null);
  const [myLibraryIssues, setMyLibraryIssues] = useState([]);
  const [myExamAllocations, setMyExamAllocations] = useState([]);
  const [notifications, setNotifications] = useState([]);
  const [notificationMessage, setNotificationMessage] = useState('');
  const [recommendedBooks, setRecommendedBooks] = useState([]);

  useEffect(() => {
    if (!currentUser) return;
    if (currentUser.role === 'Admin') {
      loadUsers();
    }
    if (currentUser.role === 'Librarian') {
      setUsers([]);
    }

    if (currentUser.role === 'Admin' || currentUser.role === 'Librarian' || currentUser.role === 'Exam Coordinator') {
      loadNotifications();
    }

    if (currentUser.role === 'Student') {
      loadStudentData();
      loadRecommendations();
    }
  }, [currentUser]);

  // On app start, if a token exists but no currentUser is loaded, try to fetch profile.
  useEffect(() => {
    const tryRestoreSession = async () => {
      const token = localStorage.getItem('libraryToken');
      if (!token) return;
      if (currentUser) return;
      try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/me`, { headers: { Authorization: `Bearer ${token}` } });
        const data = await response.json();
        if (!response.ok) throw new Error(data?.message || 'Failed to restore session');
        setCurrentUser(data);
        // persist for future reloads
        try { localStorage.setItem('libraryUser', JSON.stringify(data)); } catch {}
      } catch (error) {
        // token invalid/expired — clear and force login
        localStorage.removeItem('libraryToken');
        localStorage.removeItem('libraryUser');
        setCurrentUser(null);
      }
    };

    tryRestoreSession();
  }, []);

  useEffect(() => {
    if (!currentUser) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notifications`)
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', (payload) => {
      setNotifications((current) => [{ type: 'live', message: payload.message }, ...current].slice(0, 8));
    });

    connection.start().catch(() => {
      setNotifications((current) => [...current, { type: 'info', message: 'Realtime updates are unavailable right now.' }]);
    });

    return () => {
      connection.stop();
    };
  }, [currentUser]);

  const loadUsers = async () => {
    setLoadingResources((current) => ({ ...current, users: true }));
    try {
      const token = localStorage.getItem('libraryToken');
      const response = await fetch(`${API_BASE_URL}/api/Auth/users`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      if (!response.ok) {
        throw new Error(data.message || 'Failed to load users');
      }
      setUsers(data);
    } catch (error) {
      setStatus({ type: 'error', message: error.message });
    } finally {
      setLoadingResources((current) => ({ ...current, users: false }));
    }
  };

  const loadMyProfile = async () => {
    try {
      const token = localStorage.getItem('libraryToken');
      const response = await fetch(`${API_BASE_URL}/api/Auth/me`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      if (!response.ok) {
        throw new Error(data.message || 'Failed to load profile');
      }
      setCurrentUser(data);
    } catch (error) {
      setStatus({ type: 'error', message: error.message });
    }
  };

  const loadNotifications = async () => {
    setLoadingResources((current) => ({ ...current, dashboard: true }));
    try {
      const token = localStorage.getItem('libraryToken');
      const [summaryResponse, analyticsResponse] = await Promise.all([
        fetch(`${API_BASE_URL}/api/Notifications/summary`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch(`${API_BASE_URL}/api/Notifications/analytics`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);

      const summaryData = await summaryResponse.json();
      const analyticsData = await analyticsResponse.json();

      if (!summaryResponse.ok || !analyticsResponse.ok) {
        throw new Error('Failed to load dashboard data');
      }

      setNotifications(summaryData.notifications || []);
      setDashboardAnalytics(analyticsData || null);
    } catch {
      setNotifications([]);
      setDashboardAnalytics(null);
    } finally {
      setLoadingResources((current) => ({ ...current, dashboard: false }));
    }
  };

  const loadStudentData = async () => {
    setLoadingResources((current) => ({ ...current, student: true }));
    try {
      const token = localStorage.getItem('libraryToken');
      const [issuesResponse, allocationsResponse] = await Promise.all([
        fetch(`${API_BASE_URL}/api/Library/my-issues`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch(`${API_BASE_URL}/api/Exam/my-allocations`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);

      const issuesData = await issuesResponse.json();
      const allocationsData = await allocationsResponse.json();

      if (!issuesResponse.ok || !allocationsResponse.ok) {
        throw new Error('Failed to load student dashboard data');
      }

      setMyLibraryIssues(issuesData || []);
      setMyExamAllocations(allocationsData || []);
    } catch (error) {
      setStatus({ type: 'error', message: error.message });
    } finally {
      setLoadingResources((current) => ({ ...current, student: false }));
    }
  };

  const loadRecommendations = async () => {
    try {
      const token = localStorage.getItem('libraryToken');
      const response = await fetch(`${API_BASE_URL}/api/Library/recommendations`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      if (!response.ok) {
        throw new Error(data.message || 'Failed to load recommendations');
      }
      setRecommendedBooks(data.recommendations || []);
    } catch {
      setRecommendedBooks([]);
    }
  };

  const handleEditUser = (user) => {
    setEditingUserId(user.studentId);
    setUserForm({
      name: user.name,
      rollNo: user.rollNo,
      dept: user.dept,
      semester: String(user.semester),
      email: user.email,
      password: '',
      role: user.role,
      isVerified: user.isVerified,
    });
  };

  const logout = () => {
    localStorage.removeItem('libraryToken');
    localStorage.removeItem('libraryUser');
    setCurrentUser(null);
    setUsers([]);
    setStatus({ type: 'success', message: 'Logged out successfully.' });
  };

  const authActions = useAuthActions({ loginForm, setCurrentUser, setStatus, setIsSubmitting });
  const userActions = useUserActions({ userForm, editingUserId, setUserForm, setEditingUserId, setStatus, setIsSubmitting, reload: loadUsers });
  const notificationActions = useNotificationActions({ message: notificationMessage, setMessage: setNotificationMessage, setStatus, reload: loadNotifications });

  const isAdmin = currentUser && currentUser.role === 'Admin';
  const isLibrarian = currentUser && currentUser.role === 'Librarian';
  const isExamCoordinator = currentUser && currentUser.role === 'Exam Coordinator';
  const canManageExams = Boolean(currentUser && (currentUser.role === 'Admin' || currentUser.role === 'Exam Coordinator'));

  const statCards = [
    { label: 'Total students', value: dashboardAnalytics?.totalStudents ?? 0, trend: '+12.4%' },
    { label: 'Total books', value: dashboardAnalytics?.totalBooks ?? 0, trend: '+8.1%' },
    { label: 'Active issues', value: dashboardAnalytics?.activeIssues ?? 0, trend: '-4.2%' },
    { label: 'Overdue', value: dashboardAnalytics?.overdueIssues ?? 0, trend: '+2.3%' },
  ];

  if (!currentUser) {
    return <><ToastStack toasts={toasts} onDismiss={dismissToast} /><AuthView
      loginForm={loginForm}
      onLoginChange={(field, value) => setLoginForm((current) => ({ ...current, [field]: value }))}
      onLogin={authActions.login}
      status={status}
      isSubmitting={isSubmitting}
    /></>;
  }

  return (
    <div className="dashboard-shell">
      <ToastStack toasts={toasts} onDismiss={dismissToast} />
      <Sidebar
        isAdmin={isAdmin}
        activeSection={activeSection}
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed((prev) => !prev)}
        onNavigate={setActiveSection}
        currentRole={currentUser.role}
      />

      <div className="dashboard-panel">
        <Topbar
          activeSection={activeSection}
          search={topSearch}
          onSearchChange={setTopSearch}
          theme={theme}
          onToggleTheme={toggleTheme}
          currentUser={currentUser}
          onProfile={loadMyProfile}
          onLogout={logout}
          collapsed={sidebarCollapsed}
          onToggleSidebar={() => setSidebarCollapsed((prev) => !prev)}
        />

        <Suspense fallback={<div className="content-area"><div className="section active"><div className="loading-skeleton"><span className="skeleton-block skeleton-wide" /><span className="skeleton-block skeleton-wide" /></div></div></div>}>
        <div className="content-area">
          <OverviewSection
            active={currentUser.role !== 'Student' && activeSection === 'overview'}
            currentUser={currentUser}
            status={status}
            loading={loadingResources.dashboard}
            statCards={dashboardAnalytics ? statCards : []}
            myLibraryIssues={myLibraryIssues}
            myExamAllocations={myExamAllocations}
          />

          <LibrarySection active={(isAdmin || isLibrarian) && activeSection.startsWith('library-') ? activeSection : false} enabled={Boolean(isAdmin || isLibrarian)} />
          <ExamSection active={canManageExams && activeSection.startsWith('exam-') ? activeSection : false} enabled={canManageExams} />
          {(isAdmin || isLibrarian || isExamCoordinator) && <NotificationsSection
            active={activeSection === 'notifications'}
            isAdmin={isAdmin}
            notifications={notifications}
            notificationMessage={notificationMessage}
            onMessageChange={setNotificationMessage}
            onBroadcast={notificationActions.broadcast}
            status={status}
            isSubmitting={isSubmitting}
          />}

          {isAdmin && <UsersSection
            active={activeSection === 'users'}
            users={users}
            userForm={userForm}
            editingUserId={editingUserId}
            initialUserForm={initialUserForm}
            onFormChange={(field, value) => setUserForm((current) => ({ ...current, [field]: value }))}
            onSubmit={userActions.saveUser}
            onEdit={handleEditUser}
            onDelete={userActions.deleteUser}
            onCancel={() => { setEditingUserId(null); setUserForm(initialUserForm); }}
            status={status}
            isSubmitting={isSubmitting}
          />}

          <StudentActivity
            active={currentUser?.role === 'Student' && activeSection === 'overview'}
            recommendedBooks={recommendedBooks}
            libraryIssues={myLibraryIssues}
            examAllocations={myExamAllocations}
          />
        </div>
        </Suspense>
      </div>
    </div>
  );
}

export default App;
