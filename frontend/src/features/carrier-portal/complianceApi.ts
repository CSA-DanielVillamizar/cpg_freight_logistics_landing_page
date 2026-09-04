import { API_BASE, ApiError, apiClient, currentAccessToken } from '@/shared/api/client';
import type {
  ComplianceDocumentType,
  ComplianceStatusResponse,
  ProblemDetails,
  UploadComplianceDocumentResult,
} from '@/shared/api/types';

/** 5 MB, matching the server-side cap (SPEC.md US-03). */
export const MAX_UPLOAD_BYTES = 5 * 1024 * 1024;

export const ACCEPTED_MIME = ['application/pdf', 'image/jpeg'] as const;
export const ACCEPTED_EXTENSIONS = ['.pdf', '.jpg', '.jpeg'] as const;

export function validateFile(file: File): string | null {
  const name = file.name.toLowerCase();
  const okExtension = ACCEPTED_EXTENSIONS.some((extension) => name.endsWith(extension));
  const okMime =
    file.type === '' ? okExtension : (ACCEPTED_MIME as readonly string[]).includes(file.type);

  if (!okExtension || !okMime) {
    return 'Only PDF or JPG files are accepted.';
  }
  if (file.size === 0) {
    return 'The file is empty.';
  }
  if (file.size > MAX_UPLOAD_BYTES) {
    return 'The file exceeds the 5 MB limit.';
  }
  return null;
}

export const complianceApi = {
  getStatus: (): Promise<ComplianceStatusResponse> =>
    apiClient.get<ComplianceStatusResponse>('/compliance'),

  /** XHR upload so we can report progress (fetch has no upload-progress events). */
  upload: (
    file: File,
    documentType: ComplianceDocumentType,
    onProgress: (fraction: number) => void,
  ): Promise<UploadComplianceDocumentResult> =>
    new Promise((resolve, reject) => {
      const form = new FormData();
      form.append('file', file, file.name);
      form.append('documentType', documentType);

      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${API_BASE}/compliance/upload`);

      const token = currentAccessToken();
      if (token) {
        xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      }

      xhr.upload.addEventListener('progress', (event) => {
        if (event.lengthComputable) {
          onProgress(event.loaded / event.total);
        }
      });

      xhr.addEventListener('load', () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve(JSON.parse(xhr.responseText) as UploadComplianceDocumentResult);
          return;
        }
        let problem: ProblemDetails | null = null;
        try {
          problem = JSON.parse(xhr.responseText) as ProblemDetails;
        } catch {
          problem = null;
        }
        reject(new ApiError(xhr.status, problem));
      });

      xhr.addEventListener('error', () => reject(new ApiError(0, null)));
      xhr.send(form);
    }),
};
