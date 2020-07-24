package com.hideakin.textsearch.index.controller;

import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.dao.DataAccessException;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.FileGroupRequest;
import com.hideakin.textsearch.index.service.FileGroupService;
import com.hideakin.textsearch.index.service.IndexService;

@SpringBootTest
public class FileGroupControllerTests {

	private MockMvc mockMvc;

	@Mock
	private FileGroupService fileGroupService;

	@Mock
	private IndexService indexService;

	@InjectMocks
	private FileGroupController fileGroupController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(fileGroupController).build();
    }

	@Test
	public void getGroups_successful() throws Exception {
		when(fileGroupService.getGroups()).thenReturn(new FileGroupInfo[] {
				new FileGroupInfo(1, "foo"),
				new FileGroupInfo(2, "bar"),
				new FileGroupInfo(3, "baz")
		});
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/groups"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$[0].name").value("foo"))
        	.andExpect(jsonPath("$[1].name").value("bar"))
        	.andExpect(jsonPath("$[2].name").value("baz"));
	}

	@Test
	public void createGroup_successful() throws Exception {
		when(fileGroupService.createGroup("xyzzy")).thenReturn(new FileGroupInfo(4, "xyzzy"));
		doNothing().when(indexService).initialize(anyInt());
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("xyzzy"));
		mockMvc.perform(MockMvcRequestBuilders
				.post("/v1/groups")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isCreated())
			.andExpect(jsonPath("$.gid").value(4))
			.andExpect(jsonPath("$.name").value("xyzzy"));
		verify(fileGroupService, times(1)).createGroup("xyzzy");
		verify(indexService, times(1)).initialize(4);
	}

	private class CustomException extends DataAccessException {

		private static final long serialVersionUID = 1000L;

		public CustomException(String msg) {
			super(msg);
		}

	}

	@Test
	public void createGroup_constraintViolation() throws Exception {
		when(fileGroupService.createGroup("xyzzy")).thenThrow(new CustomException("constraint [file_groups_name_key]"));
		doNothing().when(indexService).initialize(anyInt());
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("xyzzy"));
		mockMvc.perform(MockMvcRequestBuilders
				.post("/v1/groups")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isBadRequest())
			.andExpect(jsonPath("$.error").value("constraint_violation"))
			.andExpect(jsonPath("$.error_description").value("Group name already exists."));
		verify(fileGroupService, times(1)).createGroup("xyzzy");
		verify(indexService, times(0)).initialize(anyInt());
	}

	@Test
	public void createGroup_otherError() throws Exception {
		when(fileGroupService.createGroup("xyzzy")).thenThrow(new CustomException("Something wrong happened."));
		doNothing().when(indexService).initialize(anyInt());
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("xyzzy"));
		mockMvc.perform(MockMvcRequestBuilders
				.post("/v1/groups")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isBadRequest())
			.andExpect(jsonPath("$.error").value("invalid_data_access"))
			.andExpect(jsonPath("$.error_description").value("Something wrong happened."));
		verify(fileGroupService, times(1)).createGroup("xyzzy");
		verify(indexService, times(0)).initialize(anyInt());
	}

	@Test
	public void updateGroup_successful() throws Exception {
		when(fileGroupService.updateGroup(4, "waldo")).thenReturn(new FileGroupInfo(4, "waldo"));
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("waldo"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/groups/4")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isOk())
			.andExpect(jsonPath("$.gid").value(4))
			.andExpect(jsonPath("$.name").value("waldo"));
		verify(fileGroupService, times(1)).updateGroup(4, "waldo");
	}

	@Test
	public void updateGroup_notFound() throws Exception {
		when(fileGroupService.updateGroup(4, "waldo")).thenReturn(null);
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("waldo"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/groups/4")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isNotFound());
		verify(fileGroupService, times(1)).updateGroup(4, "waldo");
	}

	@Test
	public void updateGroup_constraintViolation() throws Exception {
		when(fileGroupService.updateGroup(4, "waldo")).thenThrow(new CustomException("constraint [file_groups_name_key]"));
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("waldo"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/groups/4")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isBadRequest())
			.andExpect(jsonPath("$.error").value("constraint_violation"))
			.andExpect(jsonPath("$.error_description").value("Group name already exists."));
		verify(fileGroupService, times(1)).updateGroup(4, "waldo");
	}

	@Test
	public void updateGroup_otherError() throws Exception {
		when(fileGroupService.updateGroup(4, "waldo")).thenThrow(new CustomException("Something wrong happened."));
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new FileGroupRequest("waldo"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/groups/4")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isBadRequest())
			.andExpect(jsonPath("$.error").value("invalid_data_access"))
			.andExpect(jsonPath("$.error_description").value("Something wrong happened."));
		verify(fileGroupService, times(1)).updateGroup(4, "waldo");
	}

	@Test
	public void deleteGroup_successful() throws Exception {
		when(fileGroupService.deleteGroup(4)).thenReturn(new FileGroupInfo(4, "waldo"));
		doNothing().when(indexService).cleanup(anyInt());
		mockMvc.perform(MockMvcRequestBuilders
				.delete("/v1/groups/4"))
			.andExpect(status().isOk())
			.andExpect(jsonPath("$.gid").value(4))
			.andExpect(jsonPath("$.name").value("waldo"));
		verify(fileGroupService, times(1)).deleteGroup(4);
		verify(indexService, times(1)).cleanup(4);
	}

	@Test
	public void deleteGroup_notFound() throws Exception {
		when(fileGroupService.deleteGroup(4)).thenReturn(null);
		doNothing().when(indexService).cleanup(anyInt());
		mockMvc.perform(MockMvcRequestBuilders
				.delete("/v1/groups/4"))
			.andExpect(status().isNotFound());
		verify(fileGroupService, times(1)).deleteGroup(4);
		verify(indexService, times(0)).cleanup(anyInt());
	}

}
