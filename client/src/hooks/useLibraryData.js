import { useCallback, useEffect, useState } from 'react';
import { apiClient } from '../services/apiClient';

export function useLibraryData(enabled, search = '', genre = 'all') {
  const [data, setData] = useState({ books: [], members: [], issues: [], reports: null });
  const [loading, setLoading] = useState(false);

  const reload = useCallback(async () => {
    if (!enabled) return;
    setLoading(true);
    try {
      const [booksResponse, members, issues, reports] = await Promise.all([
        apiClient.get(`/api/Library/books?q=${encodeURIComponent(search)}&genre=${encodeURIComponent(genre)}&page=1&pageSize=20`),
        apiClient.get('/api/Library/members'),
        apiClient.get('/api/Library/issues'),
        apiClient.get('/api/Library/reports'),
      ]);
      setData({ books: booksResponse.items || [], members, issues, reports });
    } finally {
      setLoading(false);
    }
  }, [enabled, genre, search]);

  useEffect(() => { reload(); }, [reload]);

  return { ...data, loading, reload };
}
