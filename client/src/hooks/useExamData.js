import { useCallback, useEffect, useState } from 'react';
import { apiClient } from '../services/apiClient';

export function useExamData(enabled) {
  const [data, setData] = useState({ rooms: [], invigilators: [], exams: [], allocations: [] });
  const [loading, setLoading] = useState(false);

  const reload = useCallback(async () => {
    if (!enabled) return;
    setLoading(true);
    try {
      const [rooms, invigilators, exams, allocations] = await Promise.all([
        apiClient.get('/api/Exam/rooms'),
        apiClient.get('/api/Exam/invigilators'),
        apiClient.get('/api/Exam/exams'),
        apiClient.get('/api/Exam/allocations'),
      ]);
      setData({ rooms, invigilators, exams, allocations });
    } finally {
      setLoading(false);
    }
  }, [enabled]);

  useEffect(() => { reload(); }, [reload]);

  return { ...data, loading, reload };
}
