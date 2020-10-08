  
  
  --person
  UPDATE cp
     SET cp.IRISID = c.ID
    FROM [IRISDM].dbo.Contact c (nolock) 
    JOIN [mwrcdb].dbo.[cont_person] cp (nolock) on cp.agent_id = c.MigrationSourceID
	
	
--organization	
	  UPDATE co
     SET co.IRISID = c.ID
    FROM [IRISDM].dbo.Contact c (nolock) 
    JOIN [mwrcdb].dbo.[cont_organization] co (nolock) on co.agent_id = c.MigrationSourceID
    
    
--address    
      UPDATE pbCA
     SET pbCA.IRISID = ca.ID
    FROM [IRISDM].dbo.ContactAddress ca (nolock) 
    JOIN [mwrcdb].dbo.[cont_address] pbCA (nolock) on pbCA.agent_id = ca.MigrationSourceID

--phone (comm)    
      UPDATE cp
     SET cp.IRISID = p.ID,
         cp.scope = 'phone'
    FROM [IRISDM].dbo.[PhoneNumber] p (nolock) 
    JOIN [mwrcdb].dbo.[cont_comm_point] cp (nolock) on cp.cp_id = p.MigrationSourceID
    
--website (comm)
	UPDATE cp
     SET cp.IRISID = w.ID,
         cp.scope = 'website'
    FROM [IRISDM].dbo.Website w (nolock) 
    JOIN [mwrcdb].dbo.[cont_comm_point] cp (nolock) on cp.cp_id = w.MigrationSourceID

--email (comm)    
     UPDATE cp
     SET cp.IRISID = e.ID,
         cp.scope = 'email'
    FROM [IRISDM].dbo.email e (nolock) 
    JOIN [mwrcdb].dbo.[cont_comm_point] cp (nolock) on cp.cp_id = e.MigrationSourceID