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
import com.hideakin.textsearch.index.service.IndexService;

@RestController
public class FileGroupController {

	@Autowired
	private FileGroupService fileGroupService;

	@Autowired
	private IndexService indexService;

	@RequestMapping(value="/v1/groups",method=RequestMethod.GET)
	public FileGroupInfo[] getGroups() {
		return fileGroupService.getGroups();
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getGroup(
			@PathVariable int gid) {
		FileGroupInfo info = fileGroupService.getGroup(gid);
		if (info != null) {
			return new ResponseEntity<>(info, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/groups",method=RequestMethod.POST)
	public ResponseEntity<?> createGroup(
			@RequestBody FileGroupRequest req) {
		try {
			FileGroupInfo info = fileGroupService.createGroup(req.getName());
			indexService.initialize(info.getGid());
			return new ResponseEntity<>(info, HttpStatus.CREATED);
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
			FileGroupInfo info = fileGroupService.updateGroup(gid, req.getName());
			if (info != null) {
				return new ResponseEntity<>(info, HttpStatus.OK);
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
		FileGroupInfo info = fileGroupService.deleteGroup(gid);
		if (info != null) {
			indexService.cleanup(gid);
			return new ResponseEntity<>(info, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
