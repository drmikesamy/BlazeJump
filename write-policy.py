#!/usr/bin/env python3

import sys
import json

# Basic write policy that accepts all events
# You can modify this to implement custom filtering logic

def main():
    # Read the event from stdin
    line = sys.stdin.readline()
    if not line:
        return
    
    try:
        # Parse the incoming event
        data = json.loads(line)
        
        # Basic validation - accept all events by default
        # You can add custom logic here to reject specific events
        # For example:
        # - Check event content
        # - Validate event structure
        # - Apply rate limiting
        # - Check against blocklists
        
        # Return "accept" to allow the event, "reject" to deny it
        result = {
            "action": "accept",
            "msg": ""
        }
        
        print(json.dumps(result))
        
    except Exception as e:
        # If there's an error, reject the event
        result = {
            "action": "reject", 
            "msg": f"Error processing event: {str(e)}"
        }
        print(json.dumps(result))

if __name__ == "__main__":
    main()
