import { Button, Spinner } from "@heroui/react";
import { Download, ExternalLink, FileText } from "lucide-react";
import React from "react";

interface DocumentViewerProps {
	url: string | null;
	isLoading: boolean;
	documentName?: string;
	onClose: () => void;
	error?: string | null;
}

const getDocumentType = (url: string): 'pdf' | 'image' | 'unknown' => {
	if (!url) return 'unknown';
	
	const extension = url.split('.').pop()?.toLowerCase();
	
	if (extension === 'pdf') return 'pdf';
	if (['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg'].includes(extension || '')) return 'image';
	
	return 'unknown';
};

export const DocumentViewer: React.FC<DocumentViewerProps> = ({
	url,
	isLoading,
	documentName,
	onClose,
	error,
}) => {
	const documentType = url ? getDocumentType(url) : 'unknown';

	const handleDownload = () => {
		if (!url) return;
		
		const link = document.createElement('a');
		link.href = url;
		link.download = documentName || 'document';
		document.body.appendChild(link);
		link.click();
		document.body.removeChild(link);
	};

	const handleOpenInNewTab = () => {
		if (!url) return;
		window.open(url, '_blank');
	};

	const renderContent = () => {
		if (isLoading) {
			return (
				<div className="flex flex-col items-center justify-center h-96 space-y-4">
					<Spinner size="lg" />
					<p className="text-foreground-600">Loading document...</p>
				</div>
			);
		}

		if (error) {
			return (
				<div className="flex flex-col items-center justify-center h-96 space-y-4">
					<FileText className="w-16 h-16 text-danger-400" />
					<div className="text-center">
						<p className="text-danger-600 font-medium">Failed to load document</p>
						<p className="text-foreground-500 text-sm mt-1">{error}</p>
					</div>
				</div>
			);
		}

		if (!url) {
			return (
				<div className="flex flex-col items-center justify-center h-96 space-y-4">
					<FileText className="w-16 h-16 text-foreground-400" />
					<p className="text-foreground-600">No document URL available</p>
				</div>
			);
		}

		switch (documentType) {
			case 'pdf':
				return (
					<div className="w-full h-[600px] border rounded-lg overflow-hidden">
						<iframe
							src={url}
							className="w-full h-full"
							title={documentName || 'Document'}
							sandbox="allow-same-origin allow-scripts allow-popups allow-forms"
						>
							<p className="p-4">
								Your browser doesn't support iframe. 
								<Button
									variant="light"
									color="primary"
									onPress={handleOpenInNewTab}
									className="ml-2"
								>
									Open in new tab
								</Button>
							</p>
						</iframe>
					</div>
				);

			case 'image':
				return (
					<div className="w-full flex justify-center">
						<img
							src={url}
							alt={documentName || 'Document'}
							className="max-w-full max-h-[600px] object-contain rounded-lg"
							onError={(e) => {
								const target = e.target as HTMLImageElement;
								const parent = target.parentElement;
								if (parent) {
									target.style.display = 'none';
									parent.classList.add('flex', 'items-center', 'justify-center', 'h-96');
									parent.innerHTML = `
										<div class="text-center">
											<div class="w-16 h-16 mx-auto mb-4 text-foreground-400">
												<svg fill="currentColor" viewBox="0 0 24 24">
													<path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z" />
												</svg>
											</div>
											<p class="text-foreground-600">Failed to load image</p>
										</div>
									`;
								}
							}}
						/>
					</div>
				);

			default:
				return (
					<div className="flex flex-col items-center justify-center h-96 space-y-4">
						<FileText className="w-16 h-16 text-foreground-400" />
						<div className="text-center">
							<p className="text-foreground-600 font-medium">Preview not available</p>
							<p className="text-foreground-500 text-sm mt-1">
								This file type cannot be previewed in the browser
							</p>
							<div className="flex gap-2 mt-4">
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
	};

	return renderContent()
};
