import { useCallback } from 'react';
import { apiClient } from '../services/apiClient';

export function useAuthActions({ loginForm, setCurrentUser, setStatus, setIsSubmitting }) {
  // const login = useCallback(async (event) => { event.preventDefault(); setIsSubmitting(true); setStatus({ type: '', message: '' }); try { const data = await apiClient.post('/api/Auth/login', loginForm); localStorage.setItem('libraryToken', data.token); setCurrentUser(data.user); setStatus({ type: 'success', message: data.message || `Welcome ${data.user.name}.` }); } catch (error) { setStatus({ type: 'error', message: error.message }); } finally { setIsSubmitting(false); } }, [loginForm, setCurrentUser, setIsSubmitting, setStatus]);
  const login = useCallback(async (event) => { event.preventDefault(); setIsSubmitting(true); setStatus({ type: '', message: '' }); try { const data = await apiClient.post('/api/Auth/login', loginForm); localStorage.setItem('libraryToken', data.token); // persist user so reloads can restore session
    localStorage.setItem('libraryUser', JSON.stringify(data.user));
    setCurrentUser(data.user); setStatus({ type: 'success', message: data.message || `Welcome ${data.user.name}.` }); } catch (error) { setStatus({ type: 'error', message: error.message }); } finally { setIsSubmitting(false); } }, [loginForm, setCurrentUser, setIsSubmitting, setStatus]);
  return { login };
}
