import React from 'react';
import { 
  Card, 
  CardBody, 
  Button, 
  Table, 
  TableHeader, 
  TableColumn, 
  TableBody, 
  TableRow, 
  TableCell,
  Chip,
  Dropdown,
  DropdownTrigger,
  DropdownMenu,
  DropdownItem,
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  useDisclosure
} from '@heroui/react';
import { Upload, Eye, Archive, MoreVertical, UploadCloud } from 'lucide-react';


interface PolicyDocument {
  id: string;
  name: string;
  version: string;
  dateIndexed: string;
  status: 'Active' | 'Archived';
}

export const PolicyDocumentManager: React.FC = () => {
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const [documents, setDocuments] = React.useState<PolicyDocument[]>([
    {
      id: 'POL-001',
      name: 'Medical Coverage Policy',
      version: 'v2.3',
      dateIndexed: '2023-03-15',
      status: 'Active'
    },
    {
      id: 'POL-002',
      name: 'Claims Processing Guidelines',
      version: 'v1.8',
      dateIndexed: '2023-04-10',
      status: 'Active'
    },
    {
      id: 'POL-003',
      name: 'Fraud Detection Criteria',
      version: 'v3.1',
      dateIndexed: '2023-02-28',
      status: 'Active'
    },
    {
      id: 'POL-004',
      name: 'Medical Coverage Policy',
      version: 'v2.2',
      dateIndexed: '2022-11-05',
      status: 'Archived'
    },
    {
      id: 'POL-005',
      name: 'Provider Network Guidelines',
      version: 'v1.4',
      dateIndexed: '2023-01-20',
      status: 'Active'
    }
  ]);

  const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
  const [documentName, setDocumentName] = React.useState("");
  const [documentVersion, setDocumentVersion] = React.useState("");
  
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0]);
    }
  };
  
  const handleUpload = () => {
    // Simulate document upload and indexing
    const newDocument: PolicyDocument = {
      id: `POL-${Math.floor(Math.random() * 1000)}`,
      name: documentName,
      version: documentVersion,
      dateIndexed: new Date().toISOString().split('T')[0],
      status: 'Active'
    };
    
    setDocuments([newDocument, ...documents]);
    setSelectedFile(null);
    setDocumentName("");
    setDocumentVersion("");
    onOpenChange();
  };

  const toggleStatus = (id: string) => {
    setDocuments(documents.map(doc => {
      if (doc.id === id) {
        return {
          ...doc,
          status: doc.status === 'Active' ? 'Archived' : 'Active'
        };
      }
      return doc;
    }));
  };

  return (
    <>
      <Card className="shadow-sm border-zinc-200 border-1"> 
        <CardBody>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold">Policy Documents</h2>
            <Button 
              color="primary" 
              onPress={onOpen}
              startContent={<Upload width={16} />}
            >
              Index New Document
            </Button>
          </div>
          
          <Table 
            aria-label="Policy documents table"
            removeWrapper
          >
            <TableHeader>
              <TableColumn key="id">DOCUMENT ID</TableColumn>
              <TableColumn key="name">NAME</TableColumn>
              <TableColumn key="version">VERSION</TableColumn>
              <TableColumn key="dateIndexed">DATE INDEXED</TableColumn>
              <TableColumn key="status">STATUS</TableColumn>
              <TableColumn key="actions" className="text-right">ACTIONS</TableColumn>
            </TableHeader>
            <TableBody>
              {documents.map((doc) => (
                <TableRow key={doc.id}>
                  <TableCell>{doc.id}</TableCell>
                  <TableCell>{doc.name}</TableCell>
                  <TableCell>{doc.version}</TableCell>
                  <TableCell>{doc.dateIndexed}</TableCell>
                  <TableCell>
                    <Chip 
                      color={doc.status === 'Active' ? 'success' : 'default'} 
                      variant="flat" 
                      size="sm"
                    >
                      {doc.status}
                    </Chip>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button 
                        isIconOnly 
                        size="sm" 
                        variant="light"
                        color="primary"
                      >
                        <Eye width={16} />
                      </Button>
                      <Button 
                        isIconOnly 
                        size="sm" 
                        variant="light"
                        color={doc.status === 'Active' ? 'default' : 'success'}
                        onPress={() => toggleStatus(doc.id)}
                      >
                        <Archive width={16} />
                      </Button>
                      <Dropdown>
                        <DropdownTrigger>
                          <Button isIconOnly size="sm" variant="light">
                            <MoreVertical width={16} />
                          </Button>
                        </DropdownTrigger>
                        <DropdownMenu aria-label="Document Actions">
                          <DropdownItem key="view">View Content</DropdownItem>
                          <DropdownItem key="download">Download</DropdownItem>
                          <DropdownItem key="history">Version History</DropdownItem>
                        </DropdownMenu>
                      </Dropdown>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardBody>
      </Card>
      
      <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
        <ModalContent>
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">New Policy Document</ModalHeader>
              <ModalBody>
                <Input
                  label="Document Name"
                  placeholder="Enter document name"
                  value={documentName}
                  onValueChange={setDocumentName}
                />
                <Input
                  label="Version"
                  placeholder="e.g., v1.0"
                  value={documentVersion}
                  onValueChange={setDocumentVersion}
                />
                <div className="flex flex-col gap-2">
                  <label className="text-sm">Document File</label>
                  <div className="flex items-center justify-center w-full">
                    <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed rounded-lg cursor-pointer bg-content2 hover:bg-content3 transition-colors">
                      <div className="flex flex-col items-center justify-center pt-5 pb-6">
                        <UploadCloud className="w-8 h-8 mb-3 text-foreground-500" />
                        <p className="mb-2 text-sm text-foreground-600">
                          <span className="font-medium">Click to upload</span> or drag and drop
                        </p>
                        <p className="text-xs text-foreground-500">PDF, DOCX (Max 10MB)</p>
                      </div>
                      <input 
                        type="file" 
                        className="hidden" 
                        accept=".pdf,.docx" 
                        onChange={handleFileChange}
                      />
                    </label>
                  </div>
                  {selectedFile && (
                    <p className="text-sm text-foreground-600">
                      Selected: {selectedFile.name}
                    </p>
                  )}
                </div>
              </ModalBody>
              <ModalFooter>
                <Button variant="flat" onPress={onClose}>
                  Cancel
                </Button>
                <Button 
                  color="primary" 
                  onPress={handleUpload}
                  isDisabled={!selectedFile || !documentName || !documentVersion}
                >
                  Upload & Index
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};
