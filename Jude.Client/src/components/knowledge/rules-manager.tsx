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
  Switch,
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  Textarea,
  Select,
  SelectItem,
  useDisclosure
} from '@heroui/react';
import { Plus } from 'lucide-react';

interface Rule {
  id: string;
  name: string;
  description: string;
  condition: string;
  action: string;
  status: 'Active' | 'Inactive';
  priority: number;
}

export const RulesManager: React.FC = () => {
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const [rules, setRules] = React.useState<Rule[]>([
    {
      id: 'RULE-001',
      name: 'High Value Check',
      description: 'Flag claims with amount exceeding threshold for human review',
      condition: 'Claim.Amount > 1000',
      action: 'FlagForReview, SetSeverity=Medium',
      status: 'Active',
      priority: 1
    },
    {
      id: 'RULE-002',
      name: 'Provider Network Validation',
      description: 'Verify provider is in-network for member plan',
      condition: 'Provider.NetworkStatus != "In-Network"',
      action: 'FlagForReview, SetSeverity=High',
      status: 'Active',
      priority: 2
    },
    {
      id: 'RULE-003',
      name: 'Pediatric Service Check',
      description: 'Special handling for pediatric services',
      condition: 'Claim.ServiceCode IN ["PED-001", "PED-002"] AND Member.Age < 18',
      action: 'ApplyPediatricGuidelines, SetSeverity=Low',
      status: 'Active',
      priority: 3
    },
    {
      id: 'RULE-004',
      name: 'Duplicate Claim Detection',
      description: 'Identify potential duplicate claims',
      condition: 'EXISTS(PreviousClaim WHERE Member.ID = Current.Member.ID AND ABS(PreviousClaim.Date - Current.Date) < 30 AND PreviousClaim.ServiceCode = Current.ServiceCode)',
      action: 'FlagAsPotentialDuplicate, SetSeverity=Medium',
      status: 'Active',
      priority: 4
    },
    {
      id: 'RULE-005',
      name: 'Pre-Authorization Check',
      description: 'Verify pre-authorization for required services',
      condition: 'Claim.ServiceCode IN AuthorizationRequiredServices AND NOT EXISTS(Authorization WHERE Authorization.ID = Claim.AuthorizationID)',
      action: 'FlagForReview, SetSeverity=High, AddNote="Missing pre-authorization"',
      status: 'Inactive',
      priority: 5
    }
  ]);

  const [editingRule, setEditingRule] = React.useState<Rule | null>(null);
  const [ruleName, setRuleName] = React.useState("");
  const [ruleDescription, setRuleDescription] = React.useState("");
  const [ruleCondition, setRuleCondition] = React.useState("");
  const [ruleAction, setRuleAction] = React.useState("");
  const [rulePriority, setRulePriority] = React.useState("1");
  const [ruleStatus, setRuleStatus] = React.useState<"Active" | "Inactive">("Active");
  
  const handleOpenModal = (rule?: Rule) => {
    if (rule) {
      setEditingRule(rule);
      setRuleName(rule.name);
      setRuleDescription(rule.description);
      setRuleCondition(rule.condition);
      setRuleAction(rule.action);
      setRulePriority(rule.priority.toString());
      setRuleStatus(rule.status);
    } else {
      setEditingRule(null);
      setRuleName("");
      setRuleDescription("");
      setRuleCondition("");
      setRuleAction("");
      setRulePriority("1");
      setRuleStatus("Active");
    }
    onOpen();
  };
  
  const handleSaveRule = () => {
    const newRule: Rule = {
      id: editingRule ? editingRule.id : `RULE-${Math.floor(Math.random() * 1000)}`,
      name: ruleName,
      description: ruleDescription,
      condition: ruleCondition,
      action: ruleAction,
      status: ruleStatus,
      priority: Number.parseInt(rulePriority)
    };
    
    if (editingRule) {
      setRules(rules.map(rule => rule.id === editingRule.id ? newRule : rule));
    } else {
      setRules([...rules, newRule]);
    }
    
    onOpenChange();
  };

  const toggleRuleStatus = (id: string) => {
    setRules(rules.map(rule => {
      if (rule.id === id) {
        return {
          ...rule,
          status: rule.status === 'Active' ? 'Inactive' : 'Active'
        };
      }
      return rule;
    }));
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
              {rules.map((rule) => (
                <TableRow key={rule.id}>
                  <TableCell>{rule.name}</TableCell>
                  <TableCell>{rule.description}</TableCell>
                  <TableCell>{rule.priority}</TableCell>
                  <TableCell>
                    <Switch 
                      isSelected={rule.status === 'Active'} 
                      onValueChange={() => toggleRuleStatus(rule.id)}
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
                {editingRule ? 'Edit Rule' : 'Create New Rule'}
              </ModalHeader>
              <ModalBody>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label="Rule Name"
                    placeholder="Enter rule name"
                    value={ruleName}
                    onValueChange={setRuleName}
                  />
                  <div className="flex gap-4">
                    <Select
                      label="Priority"
                      placeholder="Select priority"
                      selectedKeys={[rulePriority]}
                      onChange={(e) => setRulePriority(e.target.value)}
                      className="flex-1"
                    >
                      {[1, 2, 3, 4, 5].map((priority) => (
                        <SelectItem key={priority.toString()}>
                          {priority}
                        </SelectItem>
                      ))}
                    </Select>
                    <div className="flex flex-col flex-1">
                      <span className="text-sm mb-1">Status</span>
                      <div className="flex items-center h-[40px]">
                        <Switch 
                          isSelected={ruleStatus === 'Active'} 
                          onValueChange={(selected) => setRuleStatus(selected ? 'Active' : 'Inactive')}
                          size="sm"
                          color="success"
                        />
                        <span className="ml-2">{ruleStatus}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <Textarea
                  label="Description"
                  placeholder="Enter rule description"
                  value={ruleDescription}
                  onValueChange={setRuleDescription}
                />
                
                <Textarea
                  label="Condition"
                  placeholder="IF (Claim.ServiceCode IN ['X', 'Y']) AND (Claim.MemberAge < 18)"
                  value={ruleCondition}
                  onValueChange={setRuleCondition}
                  minRows={3}
                />
                
                <Textarea
                  label="Action"
                  placeholder="THEN Action=FlagForReview, SetSeverity=High, AddNote='Pediatric high-value service'"
                  value={ruleAction}
                  onValueChange={setRuleAction}
                  minRows={3}
                />
              </ModalBody>
              <ModalFooter>
                <Button variant="flat" onPress={onClose}>
                  Cancel
                </Button>
                <Button 
                  color="primary" 
                  onPress={handleSaveRule}
                  isDisabled={!ruleName || !ruleCondition || !ruleAction}
                >
                  {editingRule ? 'Update Rule' : 'Create Rule'}
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};
