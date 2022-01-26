
require_relative 'vtparse_tables'

class String
    def pad(len)
        self << (" " * (len - self.length))
    end
end

File.open(ARGV[0] + "Enums.cs", "w") { |f|
    f.puts "namespace VtParser"
    f.puts "{"
    f.puts "	public enum States"
    f.puts "	{"
    f.puts "		UNDEFINED = 0,"
    $states_in_order.each_with_index { |state, i|
        f.puts "		VTPARSE_STATE_#{state.to_s.upcase} = #{i+1},"
    }
    f.puts "	}"
    f.puts
    f.puts "	public enum Actions"
    f.puts "	{"
	f.puts "		UNDEFINED = 0,"
    $actions_in_order.each_with_index { |action, i|
        f.puts "		VTPARSE_ACTION_#{action.to_s.upcase} = #{i+1},"
    }
    f.puts "	}"
    f.puts "}"
    f.puts
}

puts "Wrote Enums.cs"

File.open(ARGV[0] + "Tables.cs", "w") { |f|
    f.puts "namespace VtParser"
    f.puts "{"
    f.puts "	static class Tables"
    f.puts "	{"
    f.puts "		public static string[] ActionNames = new []"
    f.puts "		{"
    f.puts "			\"<no action>\","
    $actions_in_order.each { |action|
        f.puts "			\"#{action.to_s.upcase}\","
    }
    f.puts "		};"
    f.puts
    f.puts "		public static string[] StateNames = new []"
    f.puts "		{"
    f.puts "			\"<no state>\","
    $states_in_order.each { |state|
        f.puts "			\"#{state.to_s}\","
    }
    f.puts "		};"
    f.puts
    f.puts "		public static byte[,] StateTable = new byte[#{$states_in_order.length},256]"
    f.puts "		{"
    $states_in_order.each_with_index { |state, i|
        f.puts "			{   /* VTPARSE_STATE_#{state.to_s.upcase} = #{i} */"
        tableCount = $state_tables[state].count
        $state_tables[state].each_with_index { |state_change, i|
            if not state_change
                f.puts "				0,"
            else
                (action,) = state_change.find_all { |s| s.kind_of?(Symbol) }
                (state,)  = state_change.find_all { |s| s.kind_of?(StateTransition) }
                action_str = action ? "VTPARSE_ACTION_#{action.to_s.upcase}" : "UNDEFINED"
                state_str =  state ? "VTPARSE_STATE_#{state.to_state.to_s}" : "UNDEFINED"
                f.puts "				/*#{i.to_s.pad(3)}*/  (byte) Actions.#{action_str.pad(33)} | ((byte) States.#{state_str.pad(33)} << 4),"
            end
        }
        (tableCount..255).each { |i|
            f.puts "				0,"
        }
        f.puts "			},"
    }

    f.puts "        };"
    f.puts
	f.puts "		public static Actions[] EntryActions = new Actions[]"
	f.puts "		{"
    $states_in_order.each { |state|
        actions = $states[state]
        if actions[:on_entry]
            f.puts "		    Actions.VTPARSE_ACTION_#{actions[:on_entry].to_s.upcase}, /* #{state} */"
        else
            f.puts "		    Actions.UNDEFINED  /* none for #{state} */,"
        end
    }
    f.puts "		};"
    f.puts
	f.puts "		public static Actions[] ExitActions = new Actions[]"
	f.puts "		{"
    $states_in_order.each { |state|
        actions = $states[state]
        if actions[:on_exit]
            f.puts "    		Actions.VTPARSE_ACTION_#{actions[:on_exit].to_s.upcase}, /* #{state} */"
        else
            f.puts "	    	Actions.UNDEFINED  /* none for #{state} */,"
        end
    }
    f.puts "		};"
    f.puts "	}"
    f.puts "}"
}

puts "Wrote Tables.cs"

