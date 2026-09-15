import { useCallback } from 'react';
import { apiClient } from '../services/apiClient';

export function useExamActions({ roomForm, invigilatorForm, examForm, allocationForm, overrideSeatForm, editingRoomId, editingInvigilatorId, editingExamId, setRoomForm, setInvigilatorForm, setExamForm, setAllocationForm, setOverrideSeatForm, setEditingRoomId, setEditingInvigilatorId, setEditingExamId, setStatus, setIsSubmitting, reload, defaultExamDate }) {
  const run = useCallback(async (action, successMessage) => {
    setIsSubmitting(true);
    setStatus({ type: '', message: '' });
    try {
      const data = await action();
      setStatus({ type: 'success', message: successMessage(data) });
      await reload();
    } catch (error) {
      setStatus({ type: 'error', message: error.message });
    } finally {
      setIsSubmitting(false);
    }
  }, [reload, setIsSubmitting, setStatus]);

  const createRoom = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient[editingRoomId ? 'put' : 'post'](`/api/Exam/rooms${editingRoomId ? `/${editingRoomId}` : ''}`, { roomNo: roomForm.roomNo, capacity: Number(roomForm.capacity), benchLayout: roomForm.benchLayout }), () => editingRoomId ? 'Room updated successfully.' : 'Room created successfully.').then(() => { setRoomForm({ roomNo: '', capacity: 30, benchLayout: '10-per-bench' }); setEditingRoomId(null); });
  }, [editingRoomId, roomForm, run, setEditingRoomId, setRoomForm]);

  const deleteRoom = useCallback(async (id) => {
    if (!window.confirm('Delete this room?')) return;
    await run(() => apiClient.delete(`/api/Exam/rooms/${id}`), (data) => data.message || 'Room deleted successfully.');
  }, [run]);

  const createInvigilator = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient[editingInvigilatorId ? 'put' : 'post'](`/api/Exam/invigilators${editingInvigilatorId ? `/${editingInvigilatorId}` : ''}`, invigilatorForm), () => editingInvigilatorId ? 'Invigilator updated successfully.' : 'Invigilator created successfully.').then(() => { setInvigilatorForm({ name: '', dept: '' }); setEditingInvigilatorId(null); });
  }, [editingInvigilatorId, invigilatorForm, run, setEditingInvigilatorId, setInvigilatorForm]);

  const deleteInvigilator = useCallback(async (id) => {
    if (!window.confirm('Delete this invigilator?')) return;
    await run(() => apiClient.delete(`/api/Exam/invigilators/${id}`), (data) => data.message || 'Invigilator deleted successfully.');
  }, [run]);

  const createExam = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient[editingExamId ? 'put' : 'post'](`/api/Exam/exams${editingExamId ? `/${editingExamId}` : ''}`, { ...examForm, semester: Number(examForm.semester) }), () => editingExamId ? 'Exam updated successfully.' : 'Exam created successfully.').then(() => { setExamForm({ course: '', semester: '1', examDate: defaultExamDate, timeSlot: '09:00' }); setEditingExamId(null); });
  }, [defaultExamDate, editingExamId, examForm, run, setEditingExamId, setExamForm]);

  const deleteExam = useCallback(async (id) => {
    if (!window.confirm('Delete this exam?')) return;
    await run(() => apiClient.delete(`/api/Exam/exams/${id}`), (data) => data.message || 'Exam deleted successfully.');
  }, [run]);

  const allocateSeats = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient.post('/api/Exam/allocate', { ...allocationForm, examId: Number(allocationForm.examId), roomId: Number(allocationForm.roomId), semester: Number(allocationForm.semester), invigilatorId: allocationForm.invigilatorId ? Number(allocationForm.invigilatorId) : null, dept: allocationForm.dept || null }), (data) => `Seat allocation generated successfully. ${data.allocatedCount} seats assigned.`).then(() => setAllocationForm({ examId: '', roomId: '', invigilatorId: '', semester: '1', dept: '' }));
  }, [allocationForm, run, setAllocationForm]);

  const overrideSeat = useCallback((event) => {
    event.preventDefault();
    return run(() => apiClient.put(`/api/Exam/seat-allocations/${Number(overrideSeatForm.seatId)}`, { roomId: Number(overrideSeatForm.roomId), benchNo: Number(overrideSeatForm.benchNo), seatNo: Number(overrideSeatForm.seatNo), invigilatorId: overrideSeatForm.invigilatorId ? Number(overrideSeatForm.invigilatorId) : null }), () => 'Seat assignment overridden successfully.').then(() => setOverrideSeatForm({ seatId: '', roomId: '', benchNo: '1', seatNo: '1', invigilatorId: '' }));
  }, [overrideSeatForm, run, setOverrideSeatForm]);

  return { createRoom, deleteRoom, createInvigilator, deleteInvigilator, createExam, deleteExam, allocateSeats, overrideSeat };
}
