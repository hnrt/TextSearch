package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.dao.DataAccessException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.ErrorResponse;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.FileGroupRequest;
import com.hideakin.textsearch.index.service.FileGroupService;

@RestController
public class FileGroupController {

	@Autowired
	private FileGroupService service;

	@RequestMapping(value="/v1/groups",method=RequestMethod.GET)
	public FileGroupInfo[] getGroups() {
		return service.getGroups();
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getGroup(
			@PathVariable int gid) {
		FileGroupInfo body = service.getGroup(gid);
		if (body != null) {
			return new ResponseEntity<>(body, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/groups",method=RequestMethod.POST)
	public ResponseEntity<?> createGroup(
			@RequestBody FileGroupRequest req) {
		try {
			FileGroupInfo body = service.createGroup(req.getName());
			return new ResponseEntity<>(body, HttpStatus.CREATED);
		} catch (DataAccessException e) {
			ErrorResponse body;
			if (e.getMessage().contains("constraint [file_groups_name_key]")) {
				body = new ErrorResponse("constraint_violation", "Group name already exists.");
			} else {
				body = new ErrorResponse("invalid_data_access", e.getMessage());
			}
			return new ResponseEntity<>(body, HttpStatus.BAD_REQUEST);
		}
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.PUT)
	public ResponseEntity<?> updateGroup(
			@PathVariable int gid,
			@RequestBody FileGroupRequest req) {
		try {
			FileGroupInfo body = service.updateGroup(gid, req.getName());
			if (body != null) {
				return new ResponseEntity<>(body, HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		} catch (DataAccessException e) {
			ErrorResponse body;
			if (e.getMessage().contains("constraint [file_groups_name_key]")) {
				body = new ErrorResponse("constraint_violation", "Group name already exists.");
			} else {
				body = new ErrorResponse("invalid_data_access", e.getMessage());
			}
			return new ResponseEntity<>(body, HttpStatus.BAD_REQUEST);
		}
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteGroup(
			@PathVariable int gid) {
		FileGroupInfo body = service.deleteGroup(gid);
		if (body != null) {
			return new ResponseEntity<>(body, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
