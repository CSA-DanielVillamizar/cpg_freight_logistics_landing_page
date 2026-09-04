import { useCallback, useState } from 'react';
import { toast } from 'sonner';
import { ApiError } from '@/shared/api/client';
import type { ComplianceDocumentType, ComplianceStatusResponse } from '@/shared/api/types';
import { complianceApi, validateFile } from './complianceApi';

type Phase = 'idle' | 'uploading' | 'done' | 'error';

interface UploadState {
  phase: Phase;
  progress: number;
  error: string | null;
  upload: (file: File, documentType: ComplianceDocumentType) => Promise<void>;
}

export function useComplianceUpload(onUploaded: (status: ComplianceStatusResponse) => void): UploadState {
  const [phase, setPhase] = useState<Phase>('idle');
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const upload = useCallback(
    async (file: File, documentType: ComplianceDocumentType): Promise<void> => {
      const clientError = validateFile(file);
      if (clientError) {
        setPhase('error');
        setError(clientError);
        toast.error(clientError);
        return;
      }

      setPhase('uploading');
      setProgress(0);
      setError(null);

      try {
        await complianceApi.upload(file, documentType, setProgress);
        setProgress(1);
        setPhase('done');
        toast.success('Document uploaded — compliance status is now Under Review.');
        onUploaded(await complianceApi.getStatus());
      } catch (caught) {
        let message = 'Upload failed — check your connection.';
        if (caught instanceof ApiError) {
          const fieldErrors = Object.values(caught.problem?.errors ?? {})
            .flat()
            .join(' ');
          message = caught.problem?.detail ?? (fieldErrors || 'Upload failed.');
        }
        setPhase('error');
        setError(message);
        toast.error(message);
      }
    },
    [onUploaded],
  );

  return { phase, progress, error, upload };
}
