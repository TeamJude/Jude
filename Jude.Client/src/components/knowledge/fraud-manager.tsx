import { createFraudIndicator, getFraudIndicators } from '@/lib/services/fraud.service';
import { IndicatorStatus, type FraudIndicator } from '@/lib/types/fraud';
import {
  addToast,
  Button,
  Card,
  CardBody,
  Input,
  Modal,
  ModalBody,
  ModalContent,
  ModalFooter,
  ModalHeader,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableColumn,
  TableHeader,
  TableRow,
  Textarea,
  useDisclosure
} from '@heroui/react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from '@tanstack/react-form';
import { Plus } from 'lucide-react';
import React, { useState } from 'react';

export const FraudManager: React.FC = () => {
  const queryClient = useQueryClient();
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const [editingIndicator, setEditingIndicator] = useState<FraudIndicator | null>(null);
  const [errors, setErrors] = useState<string[]>([]);

  const { isPending, error, data } = useQuery({
    queryKey: ['fraudIndicators'],
    queryFn: () => getFraudIndicators({ page: 1, pageSize: 100 }),
  });

  const createIndicatorMutation = useMutation({
    mutationFn: (data: { name: string; description: string; status: IndicatorStatus }) =>
      createFraudIndicator(data),
    onSuccess: (response) => {
      if (response.success) {
        queryClient.invalidateQueries({ queryKey: ['fraudIndicators'] });
        addToast({
          title: 'Fraud indicator created successfully',
        });
        onOpenChange();
        form.reset();
      } else {
        setErrors(response.errors);
      }
    },
    onError: (error: Error) => {
      setErrors([error.message || 'An unexpected error occurred.']);
    },
  });

  const form = useForm({
    defaultValues: {
      name: '',
      description: '',
      status: IndicatorStatus.Active,
    },
    onSubmit: async ({ value }) => {
      setErrors([]);
      createIndicatorMutation.mutate(value);
    },
  });

  const handleOpenModal = (indicator?: FraudIndicator) => {
    setErrors([]);
    if (indicator) {
      setEditingIndicator(indicator);
      form.setFieldValue('name', indicator.name);
      form.setFieldValue('description', indicator.description);
      form.setFieldValue('status', indicator.status);
    } else {
      setEditingIndicator(null);
      form.reset();
    }
    onOpen();
  };

  const handleStatusToggle = (indicator: FraudIndicator) => {
    // TODO: Implement status update functionality
    console.log('Toggle status for:', indicator.id);
  };

  return (
    <>
      <Card className="shadow-sm border-zinc-200 border-1">
        <CardBody>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold">Fraud Detection Criteria</h2>
            <Button 
              color="primary" 
              onPress={() => handleOpenModal()}
              startContent={<Plus width={16} />}
            >
              Create New Indicator
            </Button>
          </div>
          
          <Table 
            aria-label="Fraud detection criteria table"
            removeWrapper
          >
            <TableHeader>
              <TableColumn key="name">INDICATOR NAME</TableColumn>
              <TableColumn key="description">DESCRIPTION</TableColumn>
              <TableColumn key="status">STATUS</TableColumn>
              <TableColumn key="createdAt">CREATED</TableColumn>
              <TableColumn key="actions" className="text-right">ACTIONS</TableColumn>
            </TableHeader>
            <TableBody 
              emptyContent={isPending ? "Loading..." : error ? "Error loading indicators" : "No fraud indicators found"}
            >
              {data?.success ? data.data.fraudIndicators.map((indicator) => (
                <TableRow key={indicator.id}>
                  <TableCell className="font-medium">{indicator.name}</TableCell>
                  <TableCell className="max-w-md">
                    <div className="truncate" title={indicator.description}>
                      {indicator.description}
                    </div>
                  </TableCell>
                  <TableCell>
                    <Switch 
                      isSelected={indicator.status === IndicatorStatus.Active} 
                      onValueChange={() => handleStatusToggle(indicator)}
                      size="sm"
                      color="success"
                    />
                  </TableCell>
                  <TableCell>
                    {new Date(indicator.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button 
                        size="sm" 
                        variant="flat"
                        color="primary"
                        onPress={() => handleOpenModal(indicator)}
                      >
                        Edit
                      </Button>
                      <Button 
                        size="sm" 
                        variant="light"
                        color="primary"
                      >
                        View
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              )) : []}
            </TableBody>
          </Table>
        </CardBody>
      </Card>
      
      <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
        <ModalContent>
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                {editingIndicator ? 'Edit Fraud Indicator' : 'Create New Fraud Indicator'}
              </ModalHeader>
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  form.handleSubmit();
                }}
              >
                <ModalBody className="space-y-4">
                  {errors.length > 0 && (
                    <div className="bg-danger-50 border border-danger-200 rounded-lg p-3">
                      <ul className="list-disc ml-5 text-sm text-danger-600">
                        {errors.map((error, index) => (
                          <li key={index}>{error}</li>
                        ))}
                      </ul>
                    </div>
                  )}

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <form.Field name="name">
                      {(field) => (
                        <Input
                          label="Indicator Name"
                          placeholder="Enter fraud indicator name"
                          value={field.state.value}
                          onValueChange={(v) => field.handleChange(v)}
                          isRequired
                          isDisabled={createIndicatorMutation.isPending}
                        />
                      )}
                    </form.Field>
                    
                    <div className="flex flex-col">
                      <span className="text-sm mb-2 text-foreground-600">Status</span>
                      <form.Field name="status">
                        {(field) => (
                          <div className="flex items-center h-[40px]">
                            <Switch 
                              isSelected={field.state.value === IndicatorStatus.Active} 
                              onValueChange={(selected) => 
                                field.handleChange(selected ? IndicatorStatus.Active : IndicatorStatus.Inactive)
                              }
                              size="sm"
                              color="success"
                              isDisabled={createIndicatorMutation.isPending}
                            />
                            <span className="ml-2 text-sm">{field.state.value}</span>
                          </div>
                        )}
                      </form.Field>
                    </div>
                  </div>
                  
                  <form.Field name="description">
                    {(field) => (
                      <Textarea
                        label="Description"
                        placeholder="Enter detailed description of the fraud indicator"
                        value={field.state.value}
                        onValueChange={(v) => field.handleChange(v)}
                        rows={4}
                        isRequired
                        isDisabled={createIndicatorMutation.isPending}
                      />
                    )}
                  </form.Field>
                </ModalBody>
                <ModalFooter>
                  <Button 
                    variant="flat" 
                    onPress={onClose}
                    isDisabled={createIndicatorMutation.isPending}
                  >
                    Cancel
                  </Button>
                  <Button 
                    color="primary" 
                    type="submit"
                    isLoading={createIndicatorMutation.isPending}
                  >
                    {editingIndicator ? 'Update Indicator' : 'Create Indicator'}
                  </Button>
                </ModalFooter>
              </form>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
}; 