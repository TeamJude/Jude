import { Button, Spinner } from "@heroui/react";
import { Download, ExternalLink, X, ZoomIn, ZoomOut } from "lucide-react";
import React, { useEffect, useState } from "react";
import { Document, Page, pdfjs } from 'react-pdf';

pdfjs.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url,
).toString();

interface DocumentViewerProps {
  url: string | null;
  isLoading?: boolean;
  documentName?: string;
  error?: string | null;
  onClose?: () => void;
}

export const DocumentViewer: React.FC<DocumentViewerProps> = ({
  url,
  isLoading = false,
  documentName = "document.pdf",
  error,
  onClose,
}) => {
  const [pdfLoading, setPdfLoading] = useState(true);
  const [pdfError, setPdfError] = useState(false);
  const [zoom, setZoom] = useState(100);
  const [rotation, setRotation] = useState(0);
  const [retryCount, setRetryCount] = useState(0);

  // Reset states when URL changes
  useEffect(() => {
    if (url) {
      setPdfLoading(true);
      setPdfError(false);
      setRetryCount(0);
    }
  }, [url]);

  const handleDownload = () => {
    if (!url) return;
    
    // Create a temporary link for download
    const link = document.createElement('a');
    link.href = url;
    link.download = documentName;
    link.setAttribute('target', '_blank');
    link.setAttribute('rel', 'noopener noreferrer');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleOpenInNewTab = () => {
    if (!url) return;
    
    // Open in new tab with proper window features
    const newWindow = window.open(url, '_blank', 'noopener,noreferrer');
    if (!newWindow) {
      // Fallback if popup blocked
      window.location.href = url;
    }
  };

  const handleZoomIn = () => {
    setZoom(prev => Math.min(prev + 25, 200));
  };

  const handleZoomOut = () => {
    setZoom(prev => Math.max(prev - 25, 50));
  };

  const handleRotate = () => {
    setRotation(prev => (prev + 90) % 360);
  };

  const handleRetry = () => {
    setPdfError(false);
    setPdfLoading(true);
    setRetryCount(prev => prev + 1);
  };


  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center h-[600px] space-y-4 bg-content1 rounded-lg">
        <Spinner size="lg" color="primary" />
        <p className="text-foreground-600">Loading document...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center h-[600px] space-y-4 bg-content1 rounded-lg">
        <div className="text-center space-y-2">
          <div className="text-4xl">⚠️</div>
          <p className="text-danger-600 font-medium">Failed to load document</p>
          <p className="text-foreground-500 text-sm">{error}</p>
        </div>
        {onClose && (
          <Button color="primary" variant="flat" onPress={onClose}>
            Close
          </Button>
        )}
      </div>
    );
  }

  if (!url) {
    return (
      <div className="flex flex-col items-center justify-center h-[600px] space-y-4 bg-content1 rounded-lg">
        <div className="text-center">
          <div className="text-4xl mb-2">📄</div>
          <p className="text-foreground-600">No document URL provided</p>
        </div>
      </div>
    );
  }

  if (pdfError) {
    return (
      <div className="w-full h-[600px] border rounded-lg overflow-hidden bg-content1 flex items-center justify-center">
        <div className="text-center space-y-4 p-6">
          <div className="text-6xl">📄</div>
          <div>
            <p className="text-foreground-600 font-medium">Cannot preview document in browser</p>
            <p className="text-foreground-500 text-sm mt-1">
              {retryCount > 0 
                ? "The document may require special permissions or be in an unsupported format"
                : "Your browser settings may prevent document viewing"
              }
            </p>
          </div>
          <div className="flex gap-2 justify-center flex-wrap">
            {retryCount < 2 && (
              <Button
                color="secondary"
                variant="flat"
                onPress={handleRetry}
              >
                Retry
              </Button>
            )}
            <Button
              color="primary"
              variant="flat"
              onPress={handleDownload}
              startContent={<Download className="w-4 h-4" />}
            >
              Download
            </Button>
            <Button
              color="primary"
              variant="light"
              onPress={handleOpenInNewTab}
              startContent={<ExternalLink className="w-4 h-4" />}
            >
              Open in New Tab
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full h-[600px] border rounded-lg overflow-hidden bg-content2 relative">
      {/* Header with document name and controls */}
      <div className="absolute top-0 left-0 right-0 z-20 bg-content1/95 backdrop-blur-sm border-b border-divider p-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3 min-w-0">
            <div className="text-lg">📄</div>
            <h3 className="font-medium text-foreground truncate">{documentName}</h3>
          </div>
          
          <div className="flex items-center gap-2">
            
                <Button
                  size="sm"
                  variant="flat"
                  isIconOnly
                  onPress={handleZoomOut}
                  isDisabled={zoom <= 50}
                  title="Zoom Out"
                >
                  <ZoomOut className="w-4 h-4" />
                </Button>
                <span className="text-sm text-foreground-600 min-w-12 text-center">
                  {zoom}%
                </span>
                <Button
                  size="sm"
                  variant="flat"
                  isIconOnly
                  onPress={handleZoomIn}
                  isDisabled={zoom >= 200}
                  title="Zoom In"
                >
                  <ZoomIn className="w-4 h-4" />
                </Button>
                <div className="w-px h-6 bg-divider mx-1" />
            
            <Button
              size="sm"
              color="primary"
              variant="flat"
              onPress={handleDownload}
              startContent={<Download className="w-3 h-3" />}
            >
              Download
            </Button>
            <Button
              size="sm"
              color="primary"
              variant="light"
              onPress={handleOpenInNewTab}
              startContent={<ExternalLink className="w-3 h-3" />}
            >
              Open
            </Button>
            
            {onClose && (
              <Button
                size="sm"
                variant="light"
                isIconOnly
                onPress={onClose}
                title="Close"
              >
                <X className="w-4 h-4" />
              </Button>
            )}
          </div>
        </div>
      </div>

      {pdfLoading && (
        <div className="absolute inset-0 flex items-center justify-center bg-content1/80 backdrop-blur-sm z-10">
          <div className="text-center space-y-4">
            <Spinner size="lg" color="primary" />
            <p className="text-foreground-600">Loading document...</p>
          </div>
        </div>
      )}
      
      <div className="pt-16 h-full">
		<Document file={url} onLoadSuccess={()=>{
					setPdfError(false);
					setPdfLoading(false);
		}}>
        <Page pageNumber={1} />
      </Document>
      </div>
    </div>
  );
};