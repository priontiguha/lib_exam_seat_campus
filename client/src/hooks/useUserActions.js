import { useCallback } from 'react';
import { apiClient } from '../services/apiClient';

export function useUserActions({ userForm, editingUserId, setUserForm, setEditingUserId, setStatus, setIsSubmitting, reload }) {
  const saveUser = useCallback(async (event) => {
    event.preventDefault(); setIsSubmitting(true); setStatus({ type: '', message: '' });
    try { await apiClient[editingUserId ? 'put' : 'post'](editingUserId ? `/api/Auth/admin/users/${editingUserId}` : '/api/Auth/admin/create-user', { ...userForm, semester: Number(userForm.semester) }); setStatus({ type: 'success', message: editingUserId ? 'User updated successfully.' : `${userForm.role} account created successfully.` }); setUserForm({ name: '', rollNo: '', dept: '', semester: '1', email: '', password: '', role: 'Student', isVerified: false }); setEditingUserId(null); await reload(); } catch (error) { setStatus({ type: 'error', message: error.message }); } finally { setIsSubmitting(false); }
  }, [editingUserId, reload, setEditingUserId, setIsSubmitting, setStatus, setUserForm, userForm]);
  const deleteUser = useCallback(async (id) => { try { await apiClient.delete(`/api/Auth/admin/users/${id}`); setStatus({ type: 'success', message: 'User deleted successfully.' }); await reload(); } catch (error) { setStatus({ type: 'error', message: error.message }); } }, [reload, setStatus]);
  return { saveUser, deleteUser };
}
