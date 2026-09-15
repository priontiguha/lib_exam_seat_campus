import { useState } from 'react';
import { useLibraryData } from './useLibraryData';
import { useLibraryActions } from './useLibraryActions';

export function useLibraryFeature(enabled) {
  const [search, setSearch] = useState('');
  const [genre, setGenre] = useState('all');
  const [libraryForm, setLibraryForm] = useState({ title: '', author: '', isbn: '', genre: 'Technology', copiesAvailable: 1, coverImageUrl: '', description: '', publisher: '', publishedYear: new Date().getFullYear() });
  const [editingBookId, setEditingBookId] = useState(null);
  const [issueForm, setIssueForm] = useState({ bookId: '', studentId: '', dueDays: 14 });
  const [bookDetails, setBookDetails] = useState(null);
  const [commentText, setCommentText] = useState('');
  const [reactionType, setReactionType] = useState('like');
  const [status, setStatus] = useState({ type: '', message: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const data = useLibraryData(enabled, search, genre);
  const actions = useLibraryActions({ libraryForm, issueForm, editingBookId, bookDetails, commentText, setLibraryForm, setIssueForm, setEditingBookId, setBookDetails, setCommentText, setReactionType, setStatus, setIsSubmitting, reload: data.reload });
  return { ...data, ...actions, search, genre, setSearch, setGenre, libraryForm, setLibraryForm, editingBookId, setEditingBookId, issueForm, setIssueForm, bookDetails, setBookDetails, commentText, setCommentText, reactionType, status, isSubmitting, setStatus };
}
