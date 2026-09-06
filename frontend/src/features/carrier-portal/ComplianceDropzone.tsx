import { useRef, useState } from 'react';
import type { DragEvent } from 'react';
import { cn } from '@/shared/lib/cn';
import type { ComplianceDocumentType, ComplianceStatusResponse } from '@/shared/api/types';
import { Button } from '@/shared/ui';
import { ACCEPTED_EXTENSIONS } from './complianceApi';
import { useComplianceUpload } from './useComplianceUpload';

const DOCUMENT_TYPES: { value: ComplianceDocumentType; label: string }[] = [
  { value: 'CertificateOfInsurance', label: 'Certificate of Insurance (COI)' },
  { value: 'GeneralLiabilityInsurance', label: 'General Liability Insurance' },
  { value: 'FdotPermit', label: 'FDOT Permit' },
  { value: 'OperatingAuthority', label: 'Operating Authority' },
  { value: 'W9', label: 'W-9' },
];

interface ComplianceDropzoneProps {
  onUploaded: (status: ComplianceStatusResponse) => void;
}

export function ComplianceDropzone({ onUploaded }: ComplianceDropzoneProps): JSX.Element {
  const inputRef = useRef<HTMLInputElement>(null);
  const [documentType, setDocumentType] = useState<ComplianceDocumentType>('CertificateOfInsurance');
  const [dragging, setDragging] = useState(false);
  const [selected, setSelected] = useState<File | null>(null);
  const { phase, progress, error, upload } = useComplianceUpload(onUploaded);

  function handleFiles(files: FileList | null): void {
    const file = files?.[0];
    if (file) {
      setSelected(file);
    }
  }

  function handleDrop(event: DragEvent<HTMLDivElement>): void {
    event.preventDefault();
    setDragging(false);
    handleFiles(event.dataTransfer.files);
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label
          htmlFor="document-type"
          className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
        >
          Document type
        </label>
        <select
          id="document-type"
          className="h-12 rounded border border-outline-strong bg-surface-card px-3 text-[16px] outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
          value={documentType}
          onChange={(event) => setDocumentType(event.target.value as ComplianceDocumentType)}
        >
          {DOCUMENT_TYPES.map((type) => (
            <option key={type.value} value={type.value}>
              {type.label}
            </option>
          ))}
        </select>
      </div>

      <div
        onDragOver={(event) => {
          event.preventDefault();
          setDragging(true);
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={handleDrop}
        className={cn(
          'flex flex-col items-center justify-center gap-2 rounded-lg border-2 border-dashed p-8 text-center transition-colors',
          dragging ? 'border-fleet-blue bg-fleet-blue-soft' : 'border-outline-strong bg-surface-muted',
        )}
      >
        <span className="material-symbols-outlined text-3xl text-steel-gray" aria-hidden>
          upload_file
        </span>
        <p className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Drag &amp; drop, or
        </p>
        <Button variant="outline" onClick={() => inputRef.current?.click()}>
          Choose file
        </Button>
        <p className="text-body-sm text-steel-gray">PDF or JPG · 5 MB max</p>
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPTED_EXTENSIONS.join(',')}
          className="hidden"
          onChange={(event) => handleFiles(event.target.files)}
        />
      </div>

      {selected ? (
        <div className="flex items-center justify-between rounded border border-slate-200 bg-surface-card p-3 shadow-sm">
          <span className="truncate font-mono text-body-sm">
            {selected.name} · {(selected.size / (1024 * 1024)).toFixed(2)} MB
          </span>
          <Button
            onClick={() => void upload(selected, documentType)}
            disabled={phase === 'uploading'}
          >
            {phase === 'uploading' ? `Uploading ${Math.round(progress * 100)}%` : 'Upload'}
          </Button>
        </div>
      ) : null}

      {phase === 'uploading' ? (
        <div className="h-2 overflow-hidden rounded-full bg-surface-muted">
          <div
            className="h-full bg-fleet-blue transition-[width] duration-200"
            style={{ width: `${Math.round(progress * 100)}%` }}
          />
        </div>
      ) : null}

      {phase === 'error' && error ? (
        <p className="text-body-sm text-error">{error}</p>
      ) : null}
    </div>
  );
}
