import { createRule, getRules } from '@/lib/services/rules.service';
import { RuleStatus } from '@/lib/types/rule';
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
  Select,
  SelectItem,
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
import { useQuery } from '@tanstack/react-query';
import { Plus } from 'lucide-react';
import React from 'react';

interface Rule {
  id: string;
  name: string;
  description: string;
  status: RuleStatus;
  priority: number;
}

export const RulesManager: React.FC = () => {
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const [rule, setRule] = React.useState<Rule & {_isEditing:boolean; _isLoading:boolean}>({
    id: '',
    name: '',
    description: '',
    status: RuleStatus.Active,
    priority: 1,
    _isEditing: false,
    _isLoading:false
  });

  const {isPending, error, data, refetch} = useQuery({
    queryKey: ['rules'],
    queryFn: ()=> getRules({ page: 1, pageSize: 100 }),
  })
  
  const handleOpenModal = (rule?: Rule) => {
    
    if (rule) {
      setRule({...rule, _isEditing: true, _isLoading: false});
    } else {
      setRule({
        id: '',
        name: '',
        description: '',
        status: RuleStatus.Active,
        priority: 1,
        _isEditing: false,
        _isLoading: false
      });
    }
    onOpen();
  };
  
  const handleSaveRule = async () => {
    rule._isLoading = true;
    if (rule._isEditing){
      //edit logic
    } else {
      const response = await createRule({
        name: rule.name,
        description: rule.description,
        priority: rule.priority,
        status: rule.status
      })

      if (response.success) {
        refetch();
        addToast({
          title:"Rule created successfully",
        })
      } else {
        addToast({
          title:"Error creating rule",
          description: response.errors.join(", "),
        })
      }
    }
    
    rule._isLoading = false;
    onOpenChange();
  };

  return (
    <>
      <Card className="shadow-sm border-zinc-200 border-1">
        <CardBody>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold">Claims Processing Rules</h2>
            <Button 
              color="primary" 
              onPress={() => handleOpenModal()}
              startContent={<Plus width={16} />}
            >
              Create New Rule
            </Button>
          </div>
          
          <Table 
            aria-label="Claims processing rules table"
            removeWrapper
          >
            <TableHeader>
              <TableColumn key="name">RULE NAME</TableColumn>
              <TableColumn key="description">DESCRIPTION</TableColumn>
              <TableColumn key="priority">PRIORITY</TableColumn>
              <TableColumn key="status">STATUS</TableColumn>
              <TableColumn key="actions" className="text-right">ACTIONS</TableColumn>
            </TableHeader>
            <TableBody>
              {/* This is cursed af and needs to be redone */}
              {isPending ?  <>Loading...</> : error ? <>Error loading rules, {error}</> : !data.success ? <>Error loading rules</> : data.data.rules.map((rule) => (
                <TableRow key={rule.id}>
                  <TableCell>{rule.name}</TableCell>
                  <TableCell>{rule.description}</TableCell>
                  <TableCell>{rule.priority}</TableCell>
                  <TableCell>
                    <Switch 
                      isSelected={rule.status === RuleStatus.Active} 
                      onValueChange={() => setRule((r) => ({ ...r, status: rule.status === RuleStatus.Active ? RuleStatus.Inactive : RuleStatus.Active }))}
                      size="sm"
                      color="success"
                    />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button 
                        size="sm" 
                        variant="flat"
                        color="primary"
                        onPress={() => handleOpenModal(rule)}
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
              ))}
            </TableBody>
          </Table>
        </CardBody>
      </Card>
      
      <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
        <ModalContent>
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                {rule._isEditing ? 'Edit Rule' : 'Create New Rule'}
              </ModalHeader>
              <ModalBody>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label="Rule Name"
                    placeholder="Enter rule name"
                    value={rule.name}
                    onValueChange={(v)=>setRule((r)=>({...r, name: v}))}
                  />
                  <div className="flex gap-4">
                    <Select
                      label="Priority"
                      placeholder="Select priority"
                      selectedKeys={[rule.priority.toString()]}
                      onChange={(e) => setRule((r) => ({ ...r, priority: Number(e.target.value) }))}
                      className="flex-1"
                    >
                      {[1, 2, 3, 4, 5].map((priority) => (
                        <SelectItem key={priority.toString()} textValue={priority.toString()}>
                          {priority}
                        </SelectItem>
                      ))}
                    </Select>
                    <div className="flex flex-col flex-1">
                      <span className="text-sm mb-1">Status</span>
                      <div className="flex items-center h-[40px]">
                        <Switch 
                          isSelected={rule.status === RuleStatus.Active} 
                          onValueChange={(selected) => setRule((r) => ({ ...r, status: selected ? RuleStatus.Active : RuleStatus.Inactive }))}
                          size="sm"
                          color="success"
                        />
                        <span className="ml-2">{rule.status}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <Textarea
                  label="Description"
                  placeholder="Enter rule description"
                  value={rule.description}
                  rows={4}
                  onValueChange={(v) => setRule((r) => ({ ...r, description: v }))}
                />
              </ModalBody>
              <ModalFooter>
                <Button variant="flat" onPress={onClose}>
                  Cancel
                </Button>
                <Button 
                  color="primary" 
                  onPress={handleSaveRule}
                  isDisabled={!rule.name || !rule.description || rule._isLoading}
                  isLoading={rule._isLoading}
                >
                  {rule._isEditing ? 'Update Rule' : 'Create Rule'}
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};
