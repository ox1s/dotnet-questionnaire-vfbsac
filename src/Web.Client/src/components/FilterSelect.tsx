import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface FilterSelectProps {
  value?: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  placeholder: string;
  options: { id: string; label: string }[];
}

export function FilterSelect({
  value,
  onChange,
  disabled,
  placeholder,
  options,
}: FilterSelectProps) {
  return (
    <Select
      value={value || "all"}
      disabled={disabled}
      onValueChange={(val) => onChange(val === "all" ? "" : val)}
    >
      <SelectTrigger className="w-full bg-slate-50 border-slate-200 text-sm ">
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent position="popper">
        <SelectGroup>
          <SelectItem value="all">{placeholder}</SelectItem>
          {options.map((item) => (
            <SelectItem key={item.id} value={item.id}>
              {item.label}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
