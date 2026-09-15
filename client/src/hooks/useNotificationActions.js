import { useCallback } from 'react';
import { apiClient } from '../services/apiClient';

export function useNotificationActions({ message, setMessage, setStatus, reload }) {
  const broadcast = useCallback(async (event) => { event.preventDefault(); if (!message.trim()) return; try { await apiClient.post('/api/Notifications/broadcast', { message: message.trim() }); setMessage(''); setStatus({ type: 'success', message: 'Broadcast sent to active sessions.' }); await reload(); } catch (error) { setStatus({ type: 'error', message: error.message }); } }, [message, reload, setMessage, setStatus]);
  return { broadcast };
}
