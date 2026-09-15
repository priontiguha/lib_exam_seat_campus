import { useState } from 'react';
import { useExamData } from './useExamData';
import { useExamActions } from './useExamActions';

const defaultExamDate = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);

export function useExamFeature(enabled) {
  const [status, setStatus] = useState({ type: '', message: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [editingRoomId, setEditingRoomId] = useState(null);
  const [editingInvigilatorId, setEditingInvigilatorId] = useState(null);
  const [editingExamId, setEditingExamId] = useState(null);
  const [roomForm, setRoomForm] = useState({ roomNo: '', capacity: 30, benchLayout: '10-per-bench' });
  const [invigilatorForm, setInvigilatorForm] = useState({ name: '', dept: '' });
  const [examForm, setExamForm] = useState({ course: '', semester: '1', examDate: defaultExamDate, timeSlot: '09:00' });
  const [allocationForm, setAllocationForm] = useState({ examId: '', roomId: '', invigilatorId: '', semester: '1', dept: '' });
  const [overrideSeatForm, setOverrideSeatForm] = useState({ seatId: '', roomId: '', benchNo: '1', seatNo: '1', invigilatorId: '' });
  const data = useExamData(enabled);
  const actions = useExamActions({ roomForm, invigilatorForm, examForm, allocationForm, overrideSeatForm, editingRoomId, editingInvigilatorId, editingExamId, setRoomForm, setInvigilatorForm, setExamForm, setAllocationForm, setOverrideSeatForm, setEditingRoomId, setEditingInvigilatorId, setEditingExamId, setStatus, setIsSubmitting, reload: data.reload, defaultExamDate });

  return { ...data, ...actions, status, isSubmitting, roomForm, invigilatorForm, examForm, allocationForm, overrideSeatForm, editingRoomId, editingInvigilatorId, editingExamId, setRoomForm, setInvigilatorForm, setExamForm, setAllocationForm, setOverrideSeatForm, setEditingRoomId, setEditingInvigilatorId, setEditingExamId, setStatus, defaultExamDate };
}
