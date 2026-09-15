import { useCallback } from 'react';
import { apiClient } from '../services/apiClient';

export function useLibraryActions({ libraryForm, issueForm, editingBookId, bookDetails, commentText, setLibraryForm, setIssueForm, setEditingBookId, setBookDetails, setCommentText, setReactionType, setStatus, setIsSubmitting, reload }) {
  const run = useCallback(async (action, successMessage) => {
    setIsSubmitting(true);
    setStatus({ type: '', message: '' });
    try { const data = await action(); setStatus({ type: 'success', message: typeof successMessage === 'function' ? successMessage(data) : successMessage }); await reload(); return data; }
    catch (error) { setStatus({ type: 'error', message: error.message }); return null; }
    finally { setIsSubmitting(false); }
  }, [reload, setIsSubmitting, setStatus]);

  const saveBook = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient[editingBookId ? 'put' : 'post'](`/api/Library/books${editingBookId ? `/${editingBookId}` : ''}`, { ...libraryForm, copiesAvailable: Number(libraryForm.copiesAvailable), publishedYear: Number(libraryForm.publishedYear) }), editingBookId ? 'Book updated successfully.' : 'Book added successfully.').then(() => { setEditingBookId(null); setLibraryForm({ title: '', author: '', isbn: '', genre: 'Technology', copiesAvailable: 1, coverImageUrl: '', description: '', publisher: '', publishedYear: new Date().getFullYear() }); });
  }, [editingBookId, libraryForm, run, setEditingBookId, setLibraryForm]);

  const deleteBook = useCallback((id) => run(() => apiClient.delete(`/api/Library/books/${id}`), 'Book deleted successfully.'), [run]);
  const issueBook = useCallback((event) => { event.preventDefault(); return run(() => apiClient.post('/api/Library/issues', { bookId: Number(issueForm.bookId), studentId: Number(issueForm.studentId), dueDays: Number(issueForm.dueDays) }), 'Book issued successfully.').then(() => setIssueForm({ bookId: '', studentId: '', dueDays: 14 })); }, [issueForm, run, setIssueForm]);
  const returnBook = useCallback((id) => run(() => apiClient.post(`/api/Library/issues/${id}/return`, {}), (data) => `Return processed. Fine: ${data.fineAmount}`), [run]);

  const loadBookDetails = useCallback(async (id) => { try { setBookDetails(await apiClient.get(`/api/Library/books/${id}/details`)); } catch (error) { setStatus({ type: 'error', message: error.message }); } }, [setBookDetails, setStatus]);
  const addComment = useCallback((event) => { event.preventDefault(); if (!bookDetails || !commentText.trim()) return; return run(() => apiClient.post(`/api/Library/books/${bookDetails.bookId}/comments`, { commentText: commentText.trim() }), 'Comment added.').then(() => { setCommentText(''); return loadBookDetails(bookDetails.bookId); }); }, [bookDetails, commentText, loadBookDetails, run, setCommentText]);
  const addReaction = useCallback((type) => { if (!bookDetails) return; return run(() => apiClient.post(`/api/Library/books/${bookDetails.bookId}/reactions`, { type }), 'Reaction updated.').then(() => { setReactionType(type); return loadBookDetails(bookDetails.bookId); }); }, [bookDetails, loadBookDetails, run, setReactionType]);

  return { saveBook, deleteBook, issueBook, returnBook, loadBookDetails, addComment, addReaction };
}
