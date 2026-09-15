import { useCallback, useEffect, useState } from 'react';
import { apiClient } from '../services/apiClient';

export function useDashboardData(role) {
  const [dashboardAnalytics, setDashboardAnalytics] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [studentData, setStudentData] = useState({ libraryIssues: [], examAllocations: [], recommendations: [] });
  const [loading, setLoading] = useState(false);

  const reload = useCallback(async () => {
    const managesNotifications = ['Admin', 'Librarian', 'Exam Coordinator'].includes(role);
    const isStudent = role === 'Student';
    if (!managesNotifications && !isStudent) return;
    setLoading(true);
    try {
      if (managesNotifications) {
        const [summary, analytics] = await Promise.all([
          apiClient.get('/api/Notifications/summary'),
          apiClient.get('/api/Notifications/analytics'),
        ]);
        setNotifications(summary.notifications || []);
        setDashboardAnalytics(analytics || null);
      }
      if (isStudent) {
        const [libraryIssues, examAllocations, recommendationData] = await Promise.all([
          apiClient.get('/api/Library/my-issues'),
          apiClient.get('/api/Exam/my-allocations'),
          apiClient.get('/api/Library/recommendations'),
        ]);
        setStudentData({ libraryIssues: libraryIssues || [], examAllocations: examAllocations || [], recommendations: recommendationData.recommendations || [] });
      }
    } finally {
      setLoading(false);
    }
  }, [role]);

  useEffect(() => { reload(); }, [reload]);

  return { dashboardAnalytics, notifications, ...studentData, loading, reload, setNotifications };
}
